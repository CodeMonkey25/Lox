namespace Lox.Core.Tokens.Literals;

public sealed class Identifier : Literal
{
    public Identifier(int lineNumber, string text) : base(lineNumber, text)
    {
    }
}