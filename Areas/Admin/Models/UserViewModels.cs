using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NguyenHaiDang_W345.Areas.Admin.Models;

public class AdminUserListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Roles { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
}

public class AdminUserDetailsViewModel : AdminUserListItemViewModel
{
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
}

public class AdminUserFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Email không được để trống"), EmailAddress(ErrorMessage = "Email không hợp lệ"), Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên không được để trống"), StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự"), Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tuổi không được để trống"), Range(1, 120, ErrorMessage = "Tuổi phải nằm trong khoảng từ 1 đến 120"), Display(Name = "Tuổi")]
    public int Age { get; set; }

    [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự"), Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ"), Display(Name = "Số điện thoại")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn quyền"), Display(Name = "Quyền")]
    public string Role { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6), DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string? Password { get; set; }

    [DataType(DataType.Password), Display(Name = "Xác nhận mật khẩu"), Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string? ConfirmPassword { get; set; }

    public IEnumerable<SelectListItem> RoleList { get; set; } = Enumerable.Empty<SelectListItem>();
}
