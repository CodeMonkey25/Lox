namespace Lox.Core.Tokens.Literals;

public sealed class String : Literal
{
    public String(int lineNumber, string text) : base(lineNumber, text)
    {
    }
}