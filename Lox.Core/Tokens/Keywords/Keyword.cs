namespace Lox.Core.Tokens.Keywords;

public abstract class Keyword : Token
{
    protected Keyword(int lineNumber) : base(lineNumber)
    {
    }
}