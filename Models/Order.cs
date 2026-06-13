using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace NguyenHaiDang_W345.Models;

// Bài 5 - 5.2.2: Lưu thông tin đơn hàng khi người dùng thanh toán giỏ hàng.
public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }

    [ForeignKey(nameof(UserId))]
    [ValidateNever]
    public ApplicationUser ApplicationUser { get; set; } = null!;

    public List<OrderDetail> OrderDetails { get; set; } = new();
}
