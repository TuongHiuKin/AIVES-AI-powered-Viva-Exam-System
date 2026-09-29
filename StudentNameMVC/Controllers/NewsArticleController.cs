using System.Security.Claims;
using AIVES.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StudentNameMVC.ViewModels;

namespace StudentNameMVC.Controllers;

[Authorize(Roles = "1,Staff")]
public class NewsArticleController : Controller
{
    private readonly INewsArticleService _newsService;

    public NewsArticleController(INewsArticleService newsService)
    {
        _newsService = newsService ?? throw new ArgumentNullException(nameof(newsService));
    }

    // GET: NewsArticle (Danh sách bài viết & Tìm kiếm theo từ khóa)
    [HttpGet]
    public async Task<IActionResult> Index(string? keyword)
    {
        var articles = await _newsService.SearchNewsAsync(keyword);

        var viewModel = new NewsArticleIndexViewModel
        {
            Keyword = keyword,
            Articles = articles.Select(a => new NewsArticleItemViewModel
            {
                NewsArticleId = a.NewsArticleId,
                NewsTitle = a.NewsTitle,
                NewsContent = a.NewsContent,
                CategoryId = a.CategoryId,
                CategoryName = a.Category?.CategoryName ?? "Chưa phân loại",
                NewsStatus = a.NewsStatus,
                CreatedById = a.CreatedById,
                CreatedByName = a.CreatedBy?.AccountName ?? "N/A",
                CreatedDate = a.CreatedDate,
                UpdatedById = a.UpdatedById,
                UpdatedByName = a.UpdatedBy?.AccountName,
                ModifiedDate = a.ModifiedDate,
                TagNames = a.NewsTags?
                    .Where(nt => nt.Tag != null)
                    .Select(nt => nt.Tag.TagName)
                    .ToList() ?? new List<string>()
            }).ToList()
        };

        return View(viewModel);
    }

    // GET: NewsArticle/CreateModal (Trả về partial view render bên trong modal popup tạo mới)
    [HttpGet]
    public async Task<IActionResult> CreateModal()
    {
        var model = new NewsArticleFormViewModel
        {
            NewsStatus = 1 // Mặc định kích hoạt
        };
        await PopulateDropdownsAsync(model);
        return PartialView("_CreateModalPartial", model);
    }

    // POST: NewsArticle/Create (Xử lý lưu bài viết từ Popup Modal)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewsArticleFormViewModel model)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            Response.StatusCode = 400;
            return PartialView("_CreateModalPartial", model);
        }

        try
        {
            await _newsService.CreateNewsAsync(
                model.NewsTitle,
                model.NewsContent,
                model.CategoryId,
                model.NewsStatus,
                currentUserId.Value,
                model.SelectedTagIds
            );

            TempData["SuccessMessage"] = "Tạo mới bài viết tin tức thành công!";
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownsAsync(model);
            Response.StatusCode = 400;
            return PartialView("_CreateModalPartial", model);
        }
    }

    // GET: NewsArticle/EditModal/5 (Trả về partial view modal với dữ liệu cũ của bài viết)
    [HttpGet]
    public async Task<IActionResult> EditModal(int id)
    {
        if (id <= 0) return NotFound();

        var article = await _newsService.GetNewsByIdAsync(id);
        if (article == null) return NotFound();

        var model = new NewsArticleFormViewModel
        {
            NewsArticleId = article.NewsArticleId,
            NewsTitle = article.NewsTitle,
            NewsContent = article.NewsContent,
            CategoryId = article.CategoryId,
            NewsStatus = article.NewsStatus,
            SelectedTagIds = article.NewsTags?.Select(nt => nt.TagId).ToList() ?? new List<int>()
        };

        await PopulateDropdownsAsync(model);
        return PartialView("_EditModalPartial", model);
    }

    // POST: NewsArticle/Edit/5 (Xử lý cập nhật bài viết từ Popup Modal)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NewsArticleFormViewModel model)
    {
        if (id != model.NewsArticleId) return BadRequest();

        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model);
            Response.StatusCode = 400;
            return PartialView("_EditModalPartial", model);
        }

        try
        {
            var updated = await _newsService.UpdateNewsAsync(
                id,
                model.NewsTitle,
                model.NewsContent,
                model.CategoryId,
                model.NewsStatus,
                currentUserId.Value,
                model.SelectedTagIds
            );

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Cập nhật bài viết thành công!";
            return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateDropdownsAsync(model);
            Response.StatusCode = 400;
            return PartialView("_EditModalPartial", model);
        }
    }

    // GET: NewsArticle/DeleteModal/5 (Trả về partial view modal xác nhận xóa)
    [HttpGet]
    public async Task<IActionResult> DeleteModal(int id)
    {
        if (id <= 0) return NotFound();

        var article = await _newsService.GetNewsByIdAsync(id);
        if (article == null) return NotFound();

        var model = new NewsArticleDeleteViewModel
        {
            NewsArticleId = article.NewsArticleId,
            NewsTitle = article.NewsTitle
        };

        return PartialView("_DeleteConfirmPartial", model);
    }

    // POST: NewsArticle/Delete/5 (Chỉ thực hiện xóa sau khi người dùng bấm Confirm trên modal)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0) return BadRequest();

        var deleted = await _newsService.DeleteNewsAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Xóa bài viết thành công!";
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

    private async Task PopulateDropdownsAsync(NewsArticleFormViewModel model)
    {
        var categories = await _newsService.GetCategoriesForDropdownAsync();
        model.CategoryOptions = categories.Select(c => new SelectListItem
        {
            Value = c.CategoryId.ToString(),
            Text = c.CategoryName,
            Selected = c.CategoryId == model.CategoryId
        }).ToList();

        var tags = await _newsService.GetAllTagsAsync();
        model.TagOptions = tags.Select(t => new SelectListItem
        {
            Value = t.TagId.ToString(),
            Text = t.TagName,
            Selected = model.SelectedTagIds.Contains(t.TagId)
        }).ToList();
    }

    #endregion
}
