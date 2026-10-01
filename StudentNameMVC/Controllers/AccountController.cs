using AIVES.BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentNameMVC.Security;
using StudentNameMVC.ViewModels;
using Microsoft.AspNetCore.RateLimiting;

namespace StudentNameMVC.Controllers;

public sealed class AccountController(IAuthService authService, IPasswordResetEmailSender resetEmailSender,
    ILogger<AccountController> logger) : Controller
{
    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("password-reset")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var token = await authService.CreatePasswordResetTokenAsync(model.Email, ct);
            if (token is not null) await resetEmailSender.SendAsync(model.Email.Trim().ToLowerInvariant(), token, ct);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            // Do not log email addresses, tokens, or SMTP exception details.
            logger.LogError("Password reset request could not be processed ({ErrorType}).", error.GetType().Name);
        }
        ModelState.Clear();
        return View(new ForgotPasswordViewModel { Submitted = true });
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        Response.Headers["Cache-Control"] = "no-store";
        return View(new ResetPasswordViewModel { Email = email ?? string.Empty, Token = token ?? string.Empty });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("password-reset")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model, CancellationToken ct)
    {
        Response.Headers["Referrer-Policy"] = "no-referrer";
        Response.Headers["Cache-Control"] = "no-store";
        if (!ModelState.IsValid) return View(model);
        if (!await authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword, ct))
        {
            ModelState.AddModelError(string.Empty, "Liên kết khôi phục không hợp lệ, đã hết hạn hoặc đã được sử dụng.");
            return View(model);
        }
        TempData["SuccessMessage"] = "Đã đặt lại mật khẩu. Vui lòng đăng nhập bằng mật khẩu mới.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        return View(new LoginViewModel
        {
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await authService.AuthenticateAsync(
            model.Email,
            model.Password,
            cancellationToken);

        if (!result.Succeeded || result.User is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        var principal = AuthenticationClaimsFactory.CreatePrincipal(result.User);
        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            properties);

        if (Url.IsLocalUrl(model.ReturnUrl))
        {
            return LocalRedirect(model.ReturnUrl);
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [Authorize]
    [HttpGet]
    public IActionResult Dashboard() => View();

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();
}
