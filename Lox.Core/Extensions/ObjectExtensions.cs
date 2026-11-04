using System.Text.Json;

namespace Lox.Core.Extensions;

public static class ObjectExtensions
{
    public static string Dump<T>(this T obj) => obj.SerializeJson();

    public static string SerializeJson<T>(this T obj)
    {
        if (obj == null) return string.Empty;
        JsonSerializerOptions options = new() { WriteIndented = true, };
        return JsonSerializer.Serialize(obj, options);
    }
}