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
    private readonly Action<string> _logError;
    private int _tokenStart = 0;
    private int _current = 0;
    private int _lineNumber = 1;
    private int _lineStart = 0;

    public Scanner(string text, Action<string> logError)
    {
        _text = text;
        _logError = logError;
    }
    
    public List<Token> ScanTokens()
    {
        List<Token> tokens = [];
        while (!IsAtEnd()) {
            // We are at the beginning of the next token
            _tokenStart = _current;
            Token? token = ScanToken();
            if (token != null) {
                tokens.Add(token);
            }
        }

        tokens.Add(new EndOfFile() { LineNumber = _lineNumber, LinePosition = _current - _lineStart + 1 });
        
        return tokens;
    }
    
    private bool IsAtEnd() => _current >= _text.Length;
    
    private Token? ScanToken() 
    {
        int linePosition = _current - _lineStart + 1;
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
                _lineNumber++;
                _lineStart = _current;
                break;
            case '(':
                return new LeftParenthesis() { LineNumber = _lineNumber, LinePosition = linePosition };
            case ')':
                return new RightParenthesis() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '{':
                return new LeftBrace() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '}':
                return new RightBrace() { LineNumber = _lineNumber, LinePosition = linePosition };
            case ',':
                return new Comma() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '.':
                return new Dot() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '-':
                return new Minus() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '+':
                return new Plus() { LineNumber = _lineNumber, LinePosition = linePosition };
            case ';':
                return new Semicolon() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '*':
                return new Star() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '!' when next == '=':
                _current++;
                return new BangEqual() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '!':
                return new Bang() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '=' when next == '=':
                _current++;
                return new EqualEqual() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '=':
                return new Equal() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '<' when next == '=':
                _current++;
                return new LessEqual() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '<':
                return new Less() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '>' when next == '=':
                _current++;
                return new GreaterEqual() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '>':
                return new Greater() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '/' when next == '/':
                while (PeekFirstCharacter() != '\n' && !IsAtEnd()) ConsumeFirstCharacter();
                break;
            case '/':
                return new Slash() { LineNumber = _lineNumber, LinePosition = linePosition };
            case '"':
            {
                string text = ExtractString();
                return new String() { LineNumber = _lineNumber, LinePosition = linePosition, Text = text };
            }
            case char d when char.IsDigit(d):
            {
                string text = ExtractNumber();
                double number = double.Parse(text);
                return new Number() { LineNumber = _lineNumber, LinePosition = linePosition, Text = text, Value = number };
            }
            case char a when char.IsLetter(a) || a == '_':
                return BuildKeywordOrIdentifier(ExtractIdentifier(), linePosition);
            default:
                _logError($"{_lineNumber}:{linePosition} - Unexpected character '{c}'.");
                break;
        }
        
        return null;
    }

    private char ConsumeFirstCharacter() => _text[_current++];
    
    private char PeekFirstCharacter() => IsAtEnd() ? '\0' : _text[_current];
    
    private char PeekSecondCharacter() => _current + 1 >= _text.Length ? '\0' : _text[_current + 1];

    private string ExtractString() 
    {
        while (PeekFirstCharacter() != '"' && !IsAtEnd()) {
            if (PeekFirstCharacter() == '\n')
            {
                _lineNumber++;
                _lineStart = _current;
            }
            ConsumeFirstCharacter();
        }

        if (IsAtEnd()) {
            _logError($"{_lineNumber}: Unterminated string.");
            return "";
        }

        // The closing ".
        ConsumeFirstCharacter();

        // Trim the surrounding quotes.
        return _text.Substring(_tokenStart + 1, _current - _tokenStart - 2);
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

        return _text.Substring(_tokenStart, _current - _tokenStart);
    }
    
    private string ExtractIdentifier() 
    {
        while (char.IsLetterOrDigit(PeekFirstCharacter()) || PeekFirstCharacter() == '_') ConsumeFirstCharacter();

        return _text.Substring(_tokenStart, _current - _tokenStart);
    }

    private Token BuildKeywordOrIdentifier(string identifier, int linePosition)
    {
        switch (identifier.ToUpper())
        {
            case "AND":
                return new And() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "CLASS":
                return new Class() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "ELSE":
                return new Else() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "FALSE":
                return new False() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "FOR":
                return new For() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "FUN":
                return new Fun() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "IF":
                return new If() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "NIL":
                return new Nil() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "OR":
                return new Or() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "PRINT":
                return new Print() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "RETURN":
                return new Return() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "SUPER":
                return new Super() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "THIS":
                return new This() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "TRUE":
                return new True() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "VAR":
                return new Var() { LineNumber = _lineNumber, LinePosition = linePosition };
            case "WHILE":
                return new While() { LineNumber = _lineNumber, LinePosition = linePosition };
            default:
                return new Identifier() { LineNumber = _lineNumber, LinePosition = linePosition, Text = identifier };
        }
    }
}