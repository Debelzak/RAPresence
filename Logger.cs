using System;

public static class Logger
{
    public static int logLevel = 0; //0=Errors only, 1=Info, 2=Warning, 3=Debug

    public enum LogType {L_ERROR, L_INFO, L_WARN, L_DEBUG}

    private static void logMessage(LogType type, string? message, params object?[] overloads)
    {
        Console.ForegroundColor = (type == LogType.L_ERROR) ? ConsoleColor.Red :
                                  (type == LogType.L_INFO) ? ConsoleColor.Cyan :
                                  (type == LogType.L_WARN) ? ConsoleColor.Yellow :
                                  (type == LogType.L_DEBUG) ? ConsoleColor.Magenta :
                                  ConsoleColor.Gray;

        if(overloads.Length < 1)
        {
            overloads = new string?[1]{message};
            message = "{0}";
        }
        
        if(message is not null) Console.WriteLine(string.Format(type.ToString() + ": " + message, overloads));
        Console.ResetColor();
    }

    public static void Error(string? message, params object?[] overloads)
    {
        logMessage(LogType.L_ERROR, message, overloads);
    }

    public static void Info(string? message, params object?[] overloads)
    {
        if(logLevel >= 1) logMessage(LogType.L_INFO, message, overloads);
    }

    public static void Warning(string? message, params object?[] overloads)
    {
        if(logLevel >= 2) logMessage(LogType.L_WARN, message, overloads);
    }

    public static void Debug(string? message, params object?[] overloads)
    {
        if(logLevel >= 3) logMessage(LogType.L_DEBUG, message, overloads);
    }
}