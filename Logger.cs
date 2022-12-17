using System;

public static class Logger
{
    public static int logLevel = 0; //0 = Nothing, 1=Info, 2=Warning, 3 = Debug

    public static void Error(string? message, params object?[] overloads)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        if(message is not null) Console.WriteLine(string.Format(message, overloads));
        Console.ResetColor();
    }

    public static void Debug(string? message, params object?[] overloads)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        if(logLevel >= 3 && message is not null) Console.WriteLine(string.Format(message, overloads));
        Console.ResetColor();
    }

    public static void Warning(string? message, params object?[] overloads)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        if(logLevel >= 2 && message is not null) Console.WriteLine(string.Format(message, overloads));
        Console.ResetColor();
    }

    public static void Info(string? message, params object?[] overloads)
    {
        Console.ForegroundColor = ConsoleColor.Gray;
        if(logLevel >= 1 && message is not null) Console.WriteLine(string.Format(message, overloads));
        Console.ResetColor();
    }
}