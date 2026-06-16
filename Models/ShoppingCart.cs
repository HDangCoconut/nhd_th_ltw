namespace NguyenHaiDang_W345.Models;

// Bài 5 - 5.2.1: Quản lý danh sách sản phẩm đang có trong giỏ hàng.
public class ShoppingCart
{
    public List<CartItem> Items { get; set; } = new();

    public void AddItem(CartItem item)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);

        if (existingItem is not null)
        {
            existingItem.Quantity += item.Quantity;
        }
        else
        {
            Items.Add(item);
        }
    }

    public void RemoveItem(int productId)
    {
        Items.RemoveAll(i => i.ProductId == productId);
    }

    public int GetQuantity(int productId) =>
        Items.FirstOrDefault(i => i.ProductId == productId)?.Quantity ?? 0;

    public void SetItem(CartItem item, int quantity)
    {
        var existing = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing is not null) existing.Quantity = quantity;
        else { item.Quantity = quantity; Items.Add(item); }
    }

    public void ChangeQuantity(int productId, int delta)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null) return;
        item.Quantity += delta;
        if (item.Quantity <= 0) RemoveItem(productId);
    }
}
