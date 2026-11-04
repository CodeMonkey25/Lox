using Lox.Core;

namespace Lox.Console;

class Program
{
    static void Main(string[] args)
    {
        string script = "3 + 4";
        Engine engine = new(script);
        string error = engine.Run();
        
        if (!string.IsNullOrEmpty(error))
        {
            System.Console.WriteLine(error);
            Environment.Exit(65);
        }

        System.Console.WriteLine("Script executed successfully");
    }
}