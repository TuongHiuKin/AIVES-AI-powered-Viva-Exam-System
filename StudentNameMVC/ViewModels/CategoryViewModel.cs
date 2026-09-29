using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StudentNameMVC.ViewModels;

/// <summary>
/// Dùng để hiển thị từng dòng bài viết trong bảng danh sách (Index View)
/// </summary>
public class CategoryItemViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

/// <summary>
/// Model truyền cho View Index chính (bao gồm từ khóa tìm kiếm và danh sách bài)
/// </summary>
public class CategoryIndexViewModel
{
    public string? Keyword { get; set; }
    public List<CategoryItemViewModel> CategoryList { get; set; } = new();
}

/// Model dùng cho Form Create & Edit trong Popup Modal
public class CategoryFormViewModel
{
    public int CategoryId { get; set; } // 0 nếu là Tạo mới

    [Required(ErrorMessage = "Danh mục không được để trống.")]
    [Display(Name = "Danh Mục")]
    public string CategoryName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Mô tả danh mục không được vượt quá 500 ký tự.")]
    [Display(Name = "Mô Tả")]
    public string CategoryDescription { get; set; } = string.Empty;
}

/// <summary>
/// Model dùng cho Popup Modal xác nhận xóa
/// </summary>
public class CategoryDeleteViewModel
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}