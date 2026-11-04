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

        tokens.Add(new EndOfFile() { LineNumber = line });
        
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
                return new LeftParenthesis() { LineNumber = line };
            case ')':
                return new RightParenthesis() { LineNumber = line };
            case '{':
                return new LeftBrace() { LineNumber = line };
            case '}':
                return new RightBrace() { LineNumber = line };
            case ',':
                return new Comma() { LineNumber = line };
            case '.':
                return new Dot() { LineNumber = line };
            case '-':
                return new Minus() { LineNumber = line };
            case '+':
                return new Plus() { LineNumber = line };
            case ';':
                return new Semicolon() { LineNumber = line };
            case '*':
                return new Star() { LineNumber = line };
            case '!' when next == '=':
                current++;
                return new BangEqual() { LineNumber = line };
            case '!':
                return new Bang() { LineNumber = line };
            case '=' when next == '=':
                current++;
                return new EqualEqual() { LineNumber = line };
            case '=':
                return new Equal() { LineNumber = line };
            case '<' when next == '=':
                current++;
                return new LessEqual() { LineNumber = line };
            case '<':
                return new Less() { LineNumber = line };
            case '>' when next == '=':
                current++;
                return new GreaterEqual() { LineNumber = line };
            case '>':
                return new Greater() { LineNumber = line };
            case '/' when next == '/':
                while (PeekFirstCharacter() != '\n' && !IsAtEnd()) ConsumeFirstCharacter();
                break;
            case '/':
                return new Slash() { LineNumber = line };
            case '"':
            {
                string text = ExtractString();
                return new String() { LineNumber = line, Text = text };
            }
            case char d when char.IsDigit(d):
            {
                string text = ExtractNumber();
                double number = double.Parse(text);
                return new Number() { LineNumber = line, Text = text, Value = number };
            }
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
                return new And() { LineNumber = line };
            case "CLASS":
                return new Class() { LineNumber = line };
            case "ELSE":
                return new Else() { LineNumber = line };
            case "FALSE":
                return new False() { LineNumber = line };
            case "FOR":
                return new For() { LineNumber = line };
            case "FUN":
                return new Fun() { LineNumber = line };
            case "IF":
                return new If() { LineNumber = line };
            case "NIL":
                return new Nil() { LineNumber = line };
            case "OR":
                return new Or() { LineNumber = line };
            case "PRINT":
                return new Print() { LineNumber = line };
            case "RETURN":
                return new Return() { LineNumber = line };
            case "SUPER":
                return new Super() { LineNumber = line };
            case "THIS":
                return new This() { LineNumber = line };
            case "TRUE":
                return new True() { LineNumber = line };
            case "VAR":
                return new Var() { LineNumber = line };
            case "WHILE":
                return new While() { LineNumber = line };
            default:
                return new Identifier() { LineNumber = line, Text = identifier };
        }
    }
}