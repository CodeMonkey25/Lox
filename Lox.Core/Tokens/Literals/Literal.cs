namespace Lox.Core.Tokens.Literals;

public abstract class Literal : Token
{
    public string Text { get; init; }
}