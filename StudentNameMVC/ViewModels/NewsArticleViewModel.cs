using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentNameMVC.ViewModels;

/// <summary>
/// Dùng để hiển thị từng dòng bài viết trong bảng danh sách (Index View)
/// </summary>
public class NewsArticleItemViewModel
{
    public int NewsArticleId { get; set; }
    public string NewsTitle { get; set; } = string.Empty;
    public string NewsContent { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public byte NewsStatus { get; set; }
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public int? UpdatedById { get; set; }
    public string? UpdatedByName { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public List<string> TagNames { get; set; } = new();
}

/// <summary>
/// Model truyền cho View Index chính (bao gồm từ khóa tìm kiếm và danh sách bài)
/// </summary>
public class NewsArticleIndexViewModel
{
    public string? Keyword { get; set; }
    public List<NewsArticleItemViewModel> Articles { get; set; } = new();
}

/// <summary>
/// Model dùng cho Form Create & Edit trong Popup Modal
/// </summary>
public class NewsArticleFormViewModel
{
    public int NewsArticleId { get; set; } // 0 nếu là Tạo mới

    [Required(ErrorMessage = "Tiêu đề bài viết không được để trống.")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự.")]
    [Display(Name = "Tiêu đề")]
    public string NewsTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung bài viết không được để trống.")]
    [Display(Name = "Nội dung")]
    public string NewsContent { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn một chuyên mục.")]
    [Display(Name = "Chuyên mục")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
    [Display(Name = "Trạng thái")]
    public byte NewsStatus { get; set; } = 1; // Mặc định 1: Active

    [Display(Name = "Gắn nhãn (Tags)")]
    public List<int> SelectedTagIds { get; set; } = new();

    // Dữ liệu dropdown và checkbox để render trên modal
    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public List<SelectListItem> TagOptions { get; set; } = new();
}

/// <summary>
/// Model dùng cho Popup Modal xác nhận xóa
/// </summary>
public class NewsArticleDeleteViewModel
{
    public int NewsArticleId { get; set; }
    public string NewsTitle { get; set; } = string.Empty;
}
