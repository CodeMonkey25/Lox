namespace Lox.Core.Tokens.Literals;

public abstract class Literal : Token
{
    public string Text { get; }
    protected Literal(int lineNumber, string text) : base(lineNumber)
    {
        Text = text;
    }
}