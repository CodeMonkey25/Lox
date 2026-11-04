namespace Lox.Core.Tokens.Operators.Unary;

public abstract class Unary : Operator
{
    protected Unary(int lineNumber) : base(lineNumber)
    {
    }
}