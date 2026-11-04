using System.Text;
using System.Text.Json;

namespace Lox.Console.Extensions;

public static class ObjectExtensions
{
    public static void Dump<T>(this T obj)
    {
        System.Console.WriteLine(obj.SerializeYaml());
    }

    public static string SerializeJson<T>(this T obj)
    {
        if (obj == null) return string.Empty;
        JsonSerializerOptions options = new() { WriteIndented = true, };
        return JsonSerializer.Serialize(obj, options);
    }
    
    public static string SerializeYaml<T>(this T obj)
    {
        if (obj == null) return string.Empty;

        return new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build()
            .Serialize(obj);
    }

    public static string SerializeXML<T>(this T obj)
    {
        if (obj == null) return string.Empty;

        var settings = new System.Xml.XmlWriterSettings()
        {
            Indent = true,
            //IndentChars = "  ",
            NewLineChars = System.Environment.NewLine,
            NewLineHandling = System.Xml.NewLineHandling.Replace,
            //OmitXmlDeclaration = true,
            //NewLineOnAttributes = true,
            //Encoding = Encoding.Latin1,
        };

        StringBuilder sb = new System.Text.StringBuilder();
        //using (MemoryStream stream = new MemoryStream())
        //using (XmlWriter writer = XmlWriter.Create(stream, settings))
        using (var writer = System.Xml.XmlWriter.Create(sb, settings))
        {
            var xmlserializer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            xmlserializer.Serialize(writer, obj);
            return sb.ToString();
            //return settings.Encoding.GetString(stream.ToArray());
        }
    }
}
