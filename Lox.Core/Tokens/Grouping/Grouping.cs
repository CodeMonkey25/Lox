namespace Lox.Core.Tokens.Grouping;

public abstract class Grouping : Token
{
    protected Grouping(int lineNumber) : base(lineNumber)
    {
    }
}