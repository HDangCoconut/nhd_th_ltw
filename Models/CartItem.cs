namespace NguyenHaiDang_W345.Models;

// Bài 5 - 5.2.1: Đại diện cho một sản phẩm trong giỏ hàng.
public class CartItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
