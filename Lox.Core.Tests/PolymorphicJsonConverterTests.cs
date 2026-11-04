using System.Text.Json;
using Lox.Core;
using Lox.Core.Extensions;
using Lox.Core.Tokens;
using Lox.Core.Tokens.Grouping;
using Lox.Core.Tokens.Keywords;
using Lox.Core.Tokens.Literals;
using Lox.Core.Tokens.Operators.Binary;
using Lox.Core.Tokens.Operators.Unary;
using Xunit;

namespace Lox.Core.Tests;

public class PolymorphicJsonConverterTests
{
    private readonly JsonSerializerOptions _options;

    public PolymorphicJsonConverterTests()
    {
        _options = new JsonSerializerOptions { WriteIndented = true };
        _options.Converters.Add(new PolymorphicJsonConverter<Token>());
    }

    [Fact]
    public void SerializeAndDeserialize_NumberToken_RoundTripsSuccessfully()
    {
        Token original = new Number { Value = 123.45, Text = "123.45", LineNumber = 42 };
        var json = JsonSerializer.Serialize(original, _options);

        var deserialized = JsonSerializer.Deserialize<Token>(json, _options);

        Assert.NotNull(deserialized);
        var numberToken = Assert.IsType<Number>(deserialized);
        Assert.Equal(123.45, numberToken.Value);
        Assert.Equal("123.45", numberToken.Text);
        Assert.Equal(42, numberToken.LineNumber);
    }

    [Fact]
    public void SerializeAndDeserialize_StringToken_RoundTripsSuccessfully()
    {
        Token original = new Tokens.Literals.String { Text = "hello world", LineNumber = 1 };
        var json = JsonSerializer.Serialize(original, _options);

        var deserialized = JsonSerializer.Deserialize<Token>(json, _options);

        Assert.NotNull(deserialized);
        var stringToken = Assert.IsType<Tokens.Literals.String>(deserialized);
        Assert.Equal("hello world", stringToken.Text);
        Assert.Equal(1, stringToken.LineNumber);
    }

    [Fact]
    public void SerializeAndDeserialize_PlusOperator_RoundTripsSuccessfully()
    {
        Token original = new Plus { LineNumber = 7 };
        var json = JsonSerializer.Serialize(original, _options);

        var deserialized = JsonSerializer.Deserialize<Token>(json, _options);

        Assert.NotNull(deserialized);
        var plusToken = Assert.IsType<Plus>(deserialized);
        Assert.Equal(7, plusToken.LineNumber);
    }

    [Fact]
    public void SerializeAndDeserialize_ListOfTokens_RoundTripsSuccessfully()
    {
        var tokens = new List<Token>
        {
            new Var { LineNumber = 1 },
            new Identifier { Text = "counter", LineNumber = 1 },
            new Equal { LineNumber = 1 },
            new Number { Value = 10, Text = "10", LineNumber = 1 },
            new Semicolon { LineNumber = 1 },
            new EndOfFile { LineNumber = 2 }
        };

        var json = JsonSerializer.Serialize(tokens, _options);
        var deserializedList = JsonSerializer.Deserialize<List<Token>>(json, _options);

        Assert.NotNull(deserializedList);
        Assert.Equal(6, deserializedList.Count);
        Assert.IsType<Var>(deserializedList[0]);
        Assert.IsType<Identifier>(deserializedList[1]);
        Assert.IsType<Equal>(deserializedList[2]);
        Assert.IsType<Number>(deserializedList[3]);
        Assert.IsType<Semicolon>(deserializedList[4]);
        Assert.IsType<EndOfFile>(deserializedList[5]);

        Assert.Equal("counter", ((Identifier)deserializedList[1]).Text);
        Assert.Equal(10, ((Number)deserializedList[3]).Value);
    }

    [Fact]
    public void SerializeAndDeserialize_NullToken_ReturnsNull()
    {
        Token? original = null;
        var json = JsonSerializer.Serialize(original, _options);

        var deserialized = JsonSerializer.Deserialize<Token>(json, _options);

        Assert.Null(deserialized);
    }

    [Fact]
    public void Deserialize_MissingTypeName_ThrowsJsonException()
    {
        var json = "{\"LineNumber\": 5}";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Token>(json, _options));
    }

    [Fact]
    public void Deserialize_UnknownTypeName_ThrowsJsonException()
    {
        var json = "{\"TypeName\": \"NonExistent.TypeName\", \"LineNumber\": 5}";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Token>(json, _options));
    }

    [Fact]
    public void Deserialize_TypeNameNotDerivingFromT_ThrowsJsonException()
    {
        var json = "{\"TypeName\": \"System.Text.StringBuilder\"}";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Token>(json, _options));
    }

    [Fact]
    public void Deserialize_CaseInsensitivePropertyNames_WorksWhenEnabled()
    {
        var caseInsensitiveOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        caseInsensitiveOptions.Converters.Add(new PolymorphicJsonConverter<Token>());

        var json = "{\"typename\": \"Lox.Core.Tokens.Literals.Number\", \"value\": 99.9, \"text\": \"99.9\", \"linenumber\": 12}";
        var deserialized = JsonSerializer.Deserialize<Token>(json, caseInsensitiveOptions);

        Assert.NotNull(deserialized);
        var numberToken = Assert.IsType<Number>(deserialized);
        Assert.Equal(99.9, numberToken.Value);
        Assert.Equal("99.9", numberToken.Text);
        Assert.Equal(12, numberToken.LineNumber);
    }
}
