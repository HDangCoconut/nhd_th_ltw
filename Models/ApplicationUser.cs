using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NguyenHaiDang_W345.Models;

public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Range(1, 120, ErrorMessage = "Tuổi phải nằm trong khoảng từ 1 đến 120")]
    public int Age { get; set; }

    [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
    public string? Address { get; set; }
}
