namespace Lox.Core.Tokens.Operators.Binary;

public abstract class Binary : Operator
{
    protected Binary(int lineNumber) : base(lineNumber)
    {
    }
}