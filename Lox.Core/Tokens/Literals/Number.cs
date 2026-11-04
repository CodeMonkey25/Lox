namespace Lox.Core.Tokens.Literals;

public sealed class Number : Literal
{
    public double Value { get; }
    
    public Number(int lineNumber, string text) : base(lineNumber, text)
    {
        Value = double.Parse(text);
    }
}