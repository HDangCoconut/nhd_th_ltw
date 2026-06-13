namespace NguyenHaiDang_W345.Models;

// Bài 5 - 5.2.2: Lưu chi tiết từng sản phẩm trong một đơn hàng.
public class OrderDetail
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
