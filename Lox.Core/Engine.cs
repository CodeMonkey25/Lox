using System.Text;
using Lox.Core.Extensions;
using Lox.Core.Tokens;

namespace Lox.Core;

public class Engine
{
    private readonly string _script;

    public Engine(string script)
    {
        _script = script;
    }

    public string Run()
    {
        List<string> errors = [];
        Scanner scanner = new(_script, errors.Add);
        List<Token> tokens = scanner.ScanTokens();

        
        if (errors.Any()) errors.Add("");
        errors.Add(tokens.Dump());
        return string.Join("\n", errors);
    }
}