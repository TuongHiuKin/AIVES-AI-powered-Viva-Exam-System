using AIVES.BLL.Interfaces;
using AIVES.BLL.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentNameMVC.Security;
using StudentNameMVC.ViewModels;

using Microsoft.AspNetCore.RateLimiting;

namespace StudentNameMVC.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    #region Authentication (TV2 - AUTH-01, AUTH-02, FIX-03)

    [AllowAnonymous]
    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("password-reset")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model,
        [FromServices] IPasswordResetEmailSender resetEmailSender,
        [FromServices] ILogger<AccountController> logger,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var token = await _authService.CreatePasswordResetTokenAsync(model.Email, ct);
            if (token is not null) await resetEmailSender.SendAsync(model.Email.Trim().ToLowerInvariant(), token, ct);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
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
        if (!await _authService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword, ct))
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
            return RedirectToDefaultRolePage();
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

        var result = await _authService.AuthenticateAsync(
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

        return RedirectToDefaultRolePage();
    }

    [Authorize]
    [HttpGet]
    public IActionResult Dashboard()
    {
        if (User.IsInRole(ApplicationRoles.Lecturer))
        {
            return RedirectToAction("Index", "ClassReports");
        }

        if (User.IsInRole(ApplicationRoles.Staff))
        {
            return RedirectToAction("Index", "StudentReports");
        }

        return View();
    }

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

    private IActionResult RedirectToDefaultRolePage()
    {
        if (User.IsInRole(ApplicationRoles.Admin))
        {
            return RedirectToAction(nameof(Dashboard));
        }

        if (User.IsInRole(ApplicationRoles.Lecturer))
        {
            return RedirectToAction("Index", "ClassReports");
        }

        if (User.IsInRole(ApplicationRoles.Staff))
        {
            return RedirectToAction("Index", "StudentReports");
        }

        return RedirectToAction(nameof(Dashboard));
    }

    #endregion

    #region Admin Account Management (TV5 - M01: ACC-01, ACC-02, ACC-03, ACC-04, ACC-05)

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Index(
        string? keyword,
        [FromServices] ISystemAccountService accountService,
        CancellationToken ct)
    {
        var accounts = await accountService.SearchAccountsAsync(keyword, ct);

        var items = new List<AccountItemViewModel>();
        foreach (var a in accounts)
        {
            var isReferenced = await accountService.IsAccountReferencedAsync(a.AccountId, ct);
            items.Add(new AccountItemViewModel
            {
                AccountId = a.AccountId,
                AccountName = a.AccountName,
                AccountEmail = a.AccountEmail,
                AccountRole = a.AccountRole,
                IsDeleted = a.IsDeleted,
                IsReferenced = isReferenced
            });
        }

        var viewModel = new AccountIndexViewModel
        {
            Keyword = keyword,
            Accounts = items
        };

        return View(viewModel);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpGet]
    public IActionResult CreateModal()
    {
        return PartialView("_CreateModalPartial", new AccountCreateViewModel());
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AccountCreateViewModel model,
        [FromServices] ISystemAccountService accountService,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return PartialView("_CreateModalPartial", model);
        }

        try
        {
            await accountService.CreateAccountAsync(
                model.AccountName,
                model.AccountEmail,
                model.AccountPassword,
                model.AccountRole,
                ct);

            TempData["SuccessMessage"] = "Tạo mới tài khoản thành công!";
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Response.StatusCode = 400;
            return PartialView("_CreateModalPartial", model);
        }
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> EditModal(
        int id,
        [FromServices] ISystemAccountService accountService,
        CancellationToken ct)
    {
        if (id <= 0) return NotFound();

        var account = await accountService.GetAccountByIdAsync(id, includeDeleted: true, ct);
        if (account == null) return NotFound();

        var model = new AccountEditViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            AccountRole = account.AccountRole
        };

        return PartialView("_EditModalPartial", model);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        AccountEditViewModel model,
        [FromServices] ISystemAccountService accountService,
        CancellationToken ct)
    {
        if (id != model.AccountId) return BadRequest();

        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return PartialView("_EditModalPartial", model);
        }

        try
        {
            var updated = await accountService.UpdateAccountAsync(
                id,
                model.AccountName,
                model.AccountEmail,
                model.AccountRole,
                model.NewPassword,
                ct);

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Response.StatusCode = 400;
            return PartialView("_EditModalPartial", model);
        }
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> DeleteModal(
        int id,
        [FromServices] ISystemAccountService accountService,
        CancellationToken ct)
    {
        if (id <= 0) return NotFound();

        var account = await accountService.GetAccountByIdAsync(id, includeDeleted: true, ct);
        if (account == null) return NotFound();

        var model = new AccountDeleteViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            RoleName = account.AccountRole == 1 ? "Sinh viên" : "Giảng viên",
            IsHardDelete = false
        };

        return PartialView("_DeleteConfirmPartial", model);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] ISystemAccountService accountService,
        CancellationToken ct)
    {
        if (id <= 0) return BadRequest();

        var deleted = await accountService.SoftDeleteAccountAsync(id, ct);
        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Khóa tài khoản thành công!";
        return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpGet]
    public async Task<IActionResult> HardDeleteModal(
        int id,
        [FromServices] ISystemAccountService accountService,
        CancellationToken ct)
    {
        if (id <= 0) return NotFound();

        var account = await accountService.GetAccountByIdAsync(id, includeDeleted: true, ct);
        if (account == null) return NotFound();

        var isReferenced = await accountService.IsAccountReferencedAsync(id, ct);

        var model = new AccountDeleteViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            RoleName = account.AccountRole == 1 ? "Sinh viên" : "Giảng viên",
            IsHardDelete = true,
            IsReferenced = isReferenced
        };

        return PartialView("_HardDeleteConfirmPartial", model);
    }

    [Authorize(Roles = ApplicationRoles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDelete(
        int id,
        [FromServices] ISystemAccountService accountService,
        CancellationToken ct)
    {
        if (id <= 0) return BadRequest();

        var result = await accountService.HardDeleteAccountAsync(id, ct);
        if (result == AIVES.DAL.Repositories.Models.DeleteResult.InUse)
        {
            Response.StatusCode = 400;
            return Json(new
            {
                success = false,
                message = "Không thể xóa vĩnh viễn tài khoản này vì đã có bài viết tin tức tham chiếu (là Tác giả hoặc Người chỉnh sửa gần nhất)."
            });
        }

        if (result == AIVES.DAL.Repositories.Models.DeleteResult.NotFound)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Xóa vĩnh viễn tài khoản thành công!";
        return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
    }

    #endregion
}
