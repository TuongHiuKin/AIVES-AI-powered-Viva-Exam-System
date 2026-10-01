using AIVES.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentNameMVC.ViewModels;

namespace StudentNameMVC.Controllers;

[Authorize(Roles = "Admin")]
public class AccountController : Controller
{
    private readonly ISystemAccountService _accountService;

    public AccountController(ISystemAccountService accountService)
    {
        _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
    }

    #region Admin Account Management (M01 - ACC-01, ACC-02, ACC-03, ACC-04, ACC-05)

    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, byte? roleFilter, CancellationToken ct)
    {
        if (roleFilter.HasValue && roleFilter.Value is not (1 or 2))
        {
            ModelState.AddModelError(nameof(roleFilter), "Vai trò lọc không hợp lệ. Chỉ chấp nhận Sinh viên (1) hoặc Giảng viên (2).");
            roleFilter = null;
        }

        var accounts = await _accountService.SearchAccountsAsync(keyword, roleFilter, includeDeleted: true, ct);

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
                IsReferenced = isReferenced,
                StudentCode = null,
                ClassName = null
            });
        }

        var viewModel = new AccountIndexViewModel
        {
            Keyword = keyword,
            RoleFilter = roleFilter,
            Accounts = items
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult CreateModal()
    {
        return PartialView("_CreateModalPartial", new AccountCreateViewModel());
    }

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
