using Lox.Core.Tokens;
using Lox.Core.Tokens.Grouping;
using Lox.Core.Tokens.Keywords;
using Lox.Core.Tokens.Literals;
using Lox.Core.Tokens.Operators.Binary;
using Lox.Core.Tokens.Operators.Unary;

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
        Token? token = null;
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
                token = new LeftParenthesis();
                break;
            case ')':
                token = new RightParenthesis();
                break;
            case '{':
                token = new LeftBrace();
                break;
            case '}':
                token = new RightBrace();
                break;
            case ',':
                token = new Comma();
                break;
            case '.':
                token = new Dot();
                break;
            case '-':
                token = new Minus();
                break;
            case '+':
                token = new Plus();
                break;
            case ';':
                token = new Semicolon();
                break;
            case '*':
                token = new Star();
                break;
            case '!' when next == '=':
                _current++;
                token = new BangEqual();
                break;
            case '!':
                token = new Bang();
                break;
            case '=' when next == '=':
                _current++;
                token = new EqualEqual();
                break;
            case '=':
                token = new Equal();
                break;
            case '<' when next == '=':
                _current++;
                token = new LessEqual();
                break;
            case '<':
                token = new Less();
                break;
            case '>' when next == '=':
                _current++;
                token = new GreaterEqual();
                break;
            case '>':
                token = new Greater();
                break;
            case '/' when next == '/':
                while (PeekFirstCharacter() != '\n' && !IsAtEnd()) ConsumeFirstCharacter();
                break;
            case '/' when next == '*':
                ConsumeFirstCharacter();
                while (!(PeekFirstCharacter() == '*' && PeekSecondCharacter() == '/'))
                {
                    if (IsAtEnd())
                    {
                        _logError($"{_lineNumber}:{linePosition} - Unterminated comment.");
                        break;
                    }

                    if (ConsumeFirstCharacter() == '\n')
                    {
                        _lineNumber++;
                        _lineStart = _current;
                    }
                }
                if (!IsAtEnd()) ConsumeFirstCharacter();
                if (!IsAtEnd()) ConsumeFirstCharacter();
                break;
            case '/':
                token = new Slash();
                break;
            case '"':
            {
                string text = ExtractString();
                token = new Tokens.Literals.String() { Text = text };
                break;
            }
            case var _ when char.IsDigit(c):
            {
                string text = ExtractNumber();
                double number = double.Parse(text);
                token = new Number() { Text = text, Value = number };
                break;
            }
            case var _ when char.IsLetter(c) || c == '_':
                token = BuildKeywordOrIdentifier(ExtractIdentifier());
                break;
            default:
                _logError($"{_lineNumber}:{linePosition} - Unexpected character '{c}'.");
                break;
        }

        if (token != null)
        {
            token.LineNumber = _lineNumber;
            token.LinePosition = linePosition;
        }
        
        return token;
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

    private Token BuildKeywordOrIdentifier(string identifier)
    {
        Token token;
        switch (identifier.ToUpper())
        {
            case "AND":
                token = new And();
                break;
            case "CLASS":
                token = new Class();
                break;
            case "ELSE":
                token = new Else();
                break;
            case "FALSE":
                token = new False();
                break;
            case "FOR":
                token = new For();
                break;
            case "FUN":
                token = new Fun();
                break;
            case "IF":
                token = new If();
                break;
            case "NIL":
                token = new Nil();
                break;
            case "OR":
                token = new Or();
                break;
            case "PRINT":
                token = new Print();
                break;
            case "RETURN":
                token = new Return();
                break;
            case "SUPER":
                token = new Super();
                break;
            case "THIS":
                token = new This();
                break;
            case "TRUE":
                token = new True();
                break;
            case "VAR":
                token = new Var();
                break;
            case "WHILE":
                token = new While();
                break;
            default:
                token = new Identifier() { Text = identifier };
                break;
        }
        return token;
    }
}