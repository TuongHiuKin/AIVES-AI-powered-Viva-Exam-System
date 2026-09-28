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

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult CreateModal()
    {
        return PartialView("_CreateModalPartial", new AccountCreateViewModel());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccountCreateViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return PartialView("_CreateModalPartial", model);
        }

        try
        {
            await _accountService.CreateAccountAsync(
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

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> EditModal(int id, CancellationToken ct)
    {
        if (id <= 0) return NotFound();

        var account = await _accountService.GetAccountByIdAsync(id, includeDeleted: true, ct);
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

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AccountEditViewModel model, CancellationToken ct)
    {
        if (id != model.AccountId) return BadRequest();

        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return PartialView("_EditModalPartial", model);
        }

        try
        {
            var updated = await _accountService.UpdateAccountAsync(
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

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> DeleteModal(int id, CancellationToken ct)
    {
        if (id <= 0) return NotFound();

        var account = await _accountService.GetAccountByIdAsync(id, includeDeleted: true, ct);
        if (account == null) return NotFound();

        var model = new AccountDeleteViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            RoleName = account.AccountRole == 1 ? "Staff" : "Lecturer",
            IsHardDelete = false
        };

        return PartialView("_DeleteConfirmPartial", model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (id <= 0) return BadRequest();

        var deleted = await _accountService.SoftDeleteAccountAsync(id, ct);
        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Khóa tài khoản thành công!";
        return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> HardDeleteModal(int id, CancellationToken ct)
    {
        if (id <= 0) return NotFound();

        var account = await _accountService.GetAccountByIdAsync(id, includeDeleted: true, ct);
        if (account == null) return NotFound();

        var isReferenced = await _accountService.IsAccountReferencedAsync(id, ct);

        var model = new AccountDeleteViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName,
            AccountEmail = account.AccountEmail,
            RoleName = account.AccountRole == 1 ? "Staff" : "Lecturer",
            IsHardDelete = true,
            IsReferenced = isReferenced
        };

        return PartialView("_HardDeleteConfirmPartial", model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDelete(int id, CancellationToken ct)
    {
        if (id <= 0) return BadRequest();

        var result = await _accountService.HardDeleteAccountAsync(id, ct);
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
