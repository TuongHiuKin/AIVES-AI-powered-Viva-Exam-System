using System.Security.Claims;
using AIVES.BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentNameMVC.ViewModels;

namespace StudentNameMVC.Controllers;

public class AccountController : Controller
{
    private readonly ISystemAccountService _accountService;
    private readonly IConfiguration _configuration;

    public AccountController(ISystemAccountService accountService, IConfiguration configuration)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    #region Authentication (Login / Logout / AccessDenied)

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToAction(nameof(Index));
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var adminEmail = _configuration["DefaultAdmin:Email"] ?? "admin@AIVESSystem.org";
        var adminPassword = _configuration["DefaultAdmin:Password"] ?? "@@abc123@@";

        // 1. Kiểm tra tài khoản Administrator cấu hình từ appsettings.json (Assignment Rule)
        if (string.Equals(model.Email.Trim(), adminEmail.Trim(), StringComparison.OrdinalIgnoreCase) &&
            model.Password == adminPassword)
        {
            var adminClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, "0"),
                new(ClaimTypes.Name, "Administrator"),
                new(ClaimTypes.Email, adminEmail),
                new(ClaimTypes.Role, "Admin")
            };

            var adminIdentity = new ClaimsIdentity(adminClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var adminPrincipal = new ClaimsPrincipal(adminIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, adminPrincipal,
                new AuthenticationProperties { IsPersistent = model.RememberMe, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);

            return RedirectToAction(nameof(Index));
        }

        // 2. Kiểm tra tài khoản Staff / Lecturer từ Database
        var account = await _accountService.AuthenticateAsync(model.Email, model.Password, ct);
        if (account != null)
        {
            var roleName = account.AccountRole == 1 ? "Staff" : "Lecturer";
            var userClaims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.AccountId.ToString()),
                new(ClaimTypes.Name, account.AccountName),
                new(ClaimTypes.Email, account.AccountEmail),
                new(ClaimTypes.Role, roleName)
            };

            var userIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var userPrincipal = new ClaimsPrincipal(userIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal,
                new AuthenticationProperties { IsPersistent = model.RememberMe, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);

            if (roleName == "Staff")
                return RedirectToAction("Index", "NewsArticle");

            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác hoặc tài khoản đã bị khóa.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    #endregion

    #region Admin Account Management (M01 - ACC-01, ACC-02)

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, CancellationToken ct)
    {
        var accounts = await _accountService.SearchAccountsAsync(keyword, ct);

        var items = new List<AccountItemViewModel>();
        foreach (var a in accounts)
        {
            var isReferenced = await _accountService.IsAccountReferencedAsync(a.AccountId, ct);
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

    #endregion
}
