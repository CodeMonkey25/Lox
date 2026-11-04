using Lox.Core.Tokens;
using Lox.Core.Tokens.Grouping;
using Lox.Core.Tokens.Keywords;
using Lox.Core.Tokens.Literals;
using Lox.Core.Tokens.Operators.Binary;
using Lox.Core.Tokens.Operators.Unary;
using String = Lox.Core.Tokens.Literals.String;

namespace Lox.Core;

public class Scanner
{
    private readonly string _text;
    private readonly Action<string> _errors;
    private int start = 0;
    private int current = 0;
    private int line = 1;

    public Scanner(string text, Action<string> errors)
    {
        _text = text;
        _errors = errors;
    }
    
    public List<Token> ScanTokens()
    {
        List<Token> tokens = [];
        while (!IsAtEnd()) {
            // We are at the beginning of the next lexeme.
            start = current;
            Token? token = ScanToken();
            if (token != null) {
                tokens.Add(token);
            }
        }

        tokens.Add(new EndOfFile(line));
        
        return tokens;
    }
    
    private bool IsAtEnd() => current >= _text.Length;
    
    private Token? ScanToken() 
    {
        char c = ConsumeFirstCharacter();
        char next = PeekFirstCharacter();
        switch (c) 
        {
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                line++;
                break;
            case '(':
                return new LeftParenthesis(line);
            case ')':
                return new RightParenthesis(line);
            case '{':
                return new LeftBrace(line);
            case '}':
                return new RightBrace(line);
            case ',':
                return new Comma(line);
            case '.':
                return new Dot(line);
            case '-':
                return new Minus(line);
            case '+':
                return new Plus(line);
            case ';':
                return new Semicolon(line);
            case '*':
                return new Star(line);
            case '!' when next == '=':
                current++;
                return new BangEqual(line);
            case '!':
                return new Bang(line);
            case '=' when next == '=':
                current++;
                return new EqualEqual(line);
            case '=':
                return new Equal(line);
            case '<' when next == '=':
                current++;
                return new LessEqual(line);
            case '<':
                return new Less(line);
            case '>' when next == '=':
                current++;
                return new GreaterEqual(line);
            case '>':
                return new Greater(line);
            case '/' when next == '/':
                while (PeekFirstCharacter() != '\n' && !IsAtEnd()) ConsumeFirstCharacter();
                break;
            case '/':
                return new Slash(line);
            case '"':
                return new String(line, ExtractString());
            case char d when char.IsDigit(d):
                return new Number(line, ExtractNumber());
            case char a when char.IsLetter(a) || a == '_':
                return BuildKeywordOrIdentifier(ExtractIdentifier());
            default:
                _errors($"{line}: Unexpected character '{c}'.");
                break;
        }
        
        return null;
    }

    private char ConsumeFirstCharacter() => _text[current++];
    
    private char PeekFirstCharacter() => IsAtEnd() ? '\0' : _text[current];
    
    private char PeekSecondCharacter() => current + 1 >= _text.Length ? '\0' : _text[current + 1];

    private string ExtractString() 
    {
        while (PeekFirstCharacter() != '"' && !IsAtEnd()) {
            if (PeekFirstCharacter() == '\n') line++;
            ConsumeFirstCharacter();
        }

        if (IsAtEnd()) {
            _errors($"{line}: Unterminated string.");
            return "";
        }

        // The closing ".
        ConsumeFirstCharacter();

        // Trim the surrounding quotes.
        return _text.Substring(start + 1, current - start - 2);
    }

    private string ExtractNumber() 
    {
        while (char.IsDigit(PeekFirstCharacter())) ConsumeFirstCharacter();

        // Look for a fractional part.
        if (PeekFirstCharacter() == '.' && char.IsDigit(PeekSecondCharacter())) {
            // Consume the "."
            ConsumeFirstCharacter();

            while (char.IsDigit(PeekFirstCharacter())) ConsumeFirstCharacter();
        }

        return _text.Substring(start, current - start);
    }
    
    private string ExtractIdentifier() 
    {
        while (char.IsLetterOrDigit(PeekFirstCharacter()) || PeekFirstCharacter() == '_') ConsumeFirstCharacter();

        return _text.Substring(start, current - start);
    }

    private Token BuildKeywordOrIdentifier(string identifier)
    {
        switch (identifier.ToUpper())
        {
            case "AND":
                return new And(line);
            case "CLASS":
                return new Class(line);
            case "ELSE":
                return new Else(line);
            case "FALSE":
                return new False(line);
            case "FOR":
                return new For(line);
            case "FUN":
                return new Fun(line);
            case "IF":
                return new If(line);
            case "NIL":
                return new Nil(line);
            case "OR":
                return new Or(line);
            case "PRINT":
                return new Print(line);
            case "RETURN":
                return new Return(line);
            case "SUPER":
                return new Super(line);
            case "THIS":
                return new This(line);
            case "TRUE":
                return new True(line);
            case "VAR":
                return new Var(line);
            case "WHILE":
                return new While(line);
            default:
                return new Identifier(line, identifier);
        }
    }
}