namespace Lox.Core.Tokens.Operators;

public abstract class Operator : Token
{
    protected Operator(int lineNumber) : base(lineNumber)
    {
    }
}