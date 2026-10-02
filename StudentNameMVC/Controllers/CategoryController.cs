using System.Security.Claims;
using AIVES.BLL.Interfaces;
using AIVES.DAL.Repositories.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentNameMVC.ViewModels;

namespace StudentNameMVC.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));

    }

    // GET: Categories by keyword
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword)
    {
        var categories = await _categoryService.SearchCategoriesAsync(keyword);

        var viewModel = new CategoryIndexViewModel
        {
            Keyword = keyword,
            CategoryList = categories.Select(a => new CategoryItemViewModel
            {
                CategoryId = a.CategoryId,
                CategoryName = a.CategoryName
            }).ToList()
        };
        return View(viewModel);
    }

    // GET: Category/CreateModal
    [HttpGet]
    public async Task<IActionResult> CreateModal()
    {
        var model = new CategoryFormViewModel {
            CategoryId = 0
        };

        return PartialView("_CreateModalPartial", model);
    }

    // POST: Create new Category
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return PartialView("_CreateModalPartial", model);
        }

        try
        {
            await _categoryService.CreateCategoriesAsync(
                model.CategoryName,
                model.CategoryDescription
            );

            TempData["SuccessMessage"] = "Tạo danh mục mới thành công!";
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Response.StatusCode = 400;
            return PartialView("_CreateModalPartial", model);
        }
    }

    // GET: Category/CreateModal
    [HttpGet]
    public async Task<IActionResult> UpdateModal(int id)
    {
        if (id <= 0) return NotFound();

        var category = await _categoryService.GetCategoriesAsync(id);
        if (category == null) return NotFound();

        var model = new CategoryFormViewModel
        {
            CategoryId = id,
            CategoryName = category.CategoryName,
            CategoryDescription = category.CategoryDescription ?? ""
        };

        return PartialView("_CreateModalPartial", model);
    }

    // POST: Edit Category
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        if (id != model.CategoryId) return BadRequest();

        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            Response.StatusCode = 400;
            return PartialView("_EditModalPartial", model);
        }

        try
        {
            var updated = await _categoryService.UpdateCategoriesAsync(
                id,
                model.CategoryName,
                model.CategoryDescription
            );

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật thành công!";
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            Response.StatusCode = 400;
            return PartialView("_EditModalPartial", model);
        }
    }

    // GET: Category/DeleteModal
    [HttpGet]
    public async Task<IActionResult> DeleteModal(int id)
    {
        if (id <= 0) return NotFound();

        var category = await _categoryService.GetCategoriesAsync(id);
        if (category == null) return NotFound();

        var model = new CategoryDeleteViewModel
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName
        };

        return PartialView("_DeleteConfirmPartial", model);
    }

    // POST: Delete Category
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0) return BadRequest();

        var deleted = await _categoryService.DeleteCategoriesAsync(id);
        if (deleted != DeleteResult.Deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Xóa danh mục thành công!";
        return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
    }

    #region Helper Methods

    private int? GetCurrentUserId()
    {
        if (User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var claimVal = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(claimVal, out var id) && id > 0)
        {
            return id;
        }

        return null;
    }

    #endregion
}