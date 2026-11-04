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

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
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
