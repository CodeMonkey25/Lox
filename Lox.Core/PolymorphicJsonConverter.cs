using System.Reflection;
using System.Text.Json;

namespace Lox.Core;

public class PolymorphicJsonConverter<T> : System.Text.Json.Serialization.JsonConverter<T>
{
    public override bool CanConvert(Type typeToConvert)
    {
        // Leave strings, primitives/enums/structs and collections to the built-in converters.
        if (typeToConvert == typeof(string) || typeToConvert.IsValueType
                                            || typeof(System.Collections.IEnumerable).IsAssignableFrom(typeToConvert))
            return false;

        return typeof(T).IsAssignableFrom(typeToConvert);
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Expected StartObject token, got {reader.TokenType}.");
        }

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Null)
        {
            return default;
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException($"Expected JSON Object, got {root.ValueKind}.");
        }

        bool foundTypeName = false;
        string? typeName = null;
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, "TypeName", StringComparison.OrdinalIgnoreCase))
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    typeName = property.Value.GetString();
                }
                foundTypeName = true;
                break;
            }
        }

        if (!foundTypeName || string.IsNullOrEmpty(typeName))
        {
            throw new JsonException("Missing or invalid 'TypeName' property.");
        }

        var targetType = Type.GetType(typeName);
        if (targetType is null)
        {
            targetType = typeof(T).Assembly.GetType(typeName);
        }
        if (targetType is null)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                targetType = assembly.GetType(typeName);
                if (targetType is not null)
                    break;
            }
        }

        if (targetType is null)
        {
            throw new JsonException($"Could not resolve type '{typeName}'.");
        }

        if (!typeof(T).IsAssignableFrom(targetType))
        {
            throw new JsonException($"Type '{typeName}' is not assignable to '{typeof(T).FullName}'.");
        }

        var instance = Activator.CreateInstance(targetType);
        if (instance is null)
        {
            throw new JsonException($"Could not create an instance of type '{targetType.FullName}'.");
        }

        var properties = targetType.GetProperties();
        var stringComparison = options.PropertyNameCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        foreach (var jsonProp in root.EnumerateObject())
        {
            if (string.Equals(jsonProp.Name, "TypeName", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var prop = properties.FirstOrDefault(p =>
                string.Equals(p.Name, jsonProp.Name, stringComparison) &&
                p.CanWrite &&
                p.GetIndexParameters().Length == 0);

            if (prop != null)
            {
                var value = jsonProp.Value.Deserialize(prop.PropertyType, options);
                prop.SetValue(instance, value);
            }
        }

        return (T)instance;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        writer.WritePropertyName("TypeName");
        writer.WriteStringValue(value.GetType().FullName);
        foreach (var property in value.GetType().GetProperties())
        {
            // Skip write-only properties and indexers (they require index arguments).
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
                continue;
            var propertyValue = property.GetValue(value);
            writer.WritePropertyName(property.Name);
            JsonSerializer.Serialize(writer, propertyValue, options);
        }
        writer.WriteEndObject();
    }
}
