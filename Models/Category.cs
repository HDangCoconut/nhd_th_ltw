using System.ComponentModel.DataAnnotations;

namespace NguyenHaiDang_W345.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên danh mục không được để trống")]
    [StringLength(50, ErrorMessage = "Tên danh mục không được vượt quá 50 ký tự")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}