namespace ImagesToBinary.Common;

class Logger
{
    public static void LogErrors(List<string> errors)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        foreach (var error in errors)
        {
            Console.WriteLine($"- {error}");
        }
        Console.ResetColor();
    }
}
