namespace Lox.Core.Tokens;

public abstract class Token
{
    public int LineNumber { get; }

    protected Token(int lineNumber)
    {
        LineNumber = lineNumber;
    }
}