using System.ComponentModel.DataAnnotations;

namespace StudentNameMVC.ViewModels;

public class AccountItemViewModel
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountEmail { get; set; } = string.Empty;
    public byte AccountRole { get; set; }
    public string RoleName => AccountRole == 1 ? "Staff" : "Lecturer";
    public bool IsDeleted { get; set; }
    public bool IsReferenced { get; set; }
}

public class AccountIndexViewModel
{
    public string? Keyword { get; set; }
    public List<AccountItemViewModel> Accounts { get; set; } = new();
    public int TotalAccounts => Accounts.Count;
    public int TotalStaff => Accounts.Count(a => a.AccountRole == 1);
    public int TotalLecturer => Accounts.Count(a => a.AccountRole == 2);
}

public class AccountCreateViewModel
{
    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng.")]
    [StringLength(254, ErrorMessage = "Email không được vượt quá 254 ký tự.")]
    [Display(Name = "Địa chỉ Email")]
    public string AccountEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có độ dài từ 6 đến 100 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string AccountPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [DataType(DataType.Password)]
    [Compare(nameof(AccountPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn vai trò tài khoản.")]
    [Range(1, 2, ErrorMessage = "Vai trò chỉ có thể là Staff (1) hoặc Lecturer (2).")]
    [Display(Name = "Vai trò")]
    public byte AccountRole { get; set; } = 1; // Mặc định là Staff
}

public class AccountEditViewModel
{
    [Required]
    public int AccountId { get; set; }

    [Required(ErrorMessage = "Họ và tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Địa chỉ Email không đúng định dạng.")]
    [StringLength(254, ErrorMessage = "Email không được vượt quá 254 ký tự.")]
    [Display(Name = "Địa chỉ Email")]
    public string AccountEmail { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự nếu muốn thay đổi.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới (Để trống nếu giữ nguyên)")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu mới")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn vai trò tài khoản.")]
    [Range(1, 2, ErrorMessage = "Vai trò chỉ có thể là Staff (1) hoặc Lecturer (2).")]
    [Display(Name = "Vai trò")]
    public byte AccountRole { get; set; }
}

public class AccountDeleteViewModel
{
    public int AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountEmail { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsHardDelete { get; set; }
    public bool IsReferenced { get; set; }
}
