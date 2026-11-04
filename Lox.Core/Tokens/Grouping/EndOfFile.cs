namespace Lox.Core.Tokens.Grouping;

public sealed class EndOfFile : Grouping
{
    public EndOfFile(int lineNumber) : base(lineNumber)
    {
    }
}