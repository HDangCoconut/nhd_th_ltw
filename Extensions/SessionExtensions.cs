using System.Security.Claims;
using System.Text.Json;
using NguyenHaiDang_W345.Models;

namespace NguyenHaiDang_W345.Extensions;

// Bài 5 - 5.2.1: Hỗ trợ lưu và đọc object trong Session dưới dạng JSON.
public static class SessionExtensions
{
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value is null ? default : JsonSerializer.Deserialize<T>(value);
    }

    public static string GetCartKey(this HttpContext context) =>
        context.User.Identity?.IsAuthenticated == true
            ? $"Cart_{context.User.FindFirstValue(ClaimTypes.NameIdentifier)}"
            : "Cart";

    public static ShoppingCart GetShoppingCart(this HttpContext context) =>
        context.Session.GetObjectFromJson<ShoppingCart>(context.GetCartKey()) ?? new ShoppingCart();

    public static void SaveShoppingCart(this HttpContext context, ShoppingCart cart) =>
        context.Session.SetObjectAsJson(context.GetCartKey(), cart);

    public static void ClearShoppingCart(this HttpContext context) =>
        context.Session.Remove(context.GetCartKey());
}
