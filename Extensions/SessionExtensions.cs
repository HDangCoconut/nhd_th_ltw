using System.Text.Json;

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
}
