namespace Lox.Core.Tokens;

public abstract class Token
{
    public int LineNumber { get; init; }
    public int LinePosition { get; init; }
}