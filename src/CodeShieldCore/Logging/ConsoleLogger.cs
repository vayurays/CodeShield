using System;

namespace CodeShieldCore.Logging;

public class ConsoleLogger : IObfuscationLogger
{
    private readonly bool _verbose;
    private static readonly object LockObj = new();

    public ConsoleLogger(bool verbose = false)
    {
        _verbose = verbose;
    }

    public void Log(LogLevel level, string message)
    {
        if (level == LogLevel.Debug && !_verbose)
        {
            return;
        }

        lock (LockObj)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = level switch
                {
                    LogLevel.Debug => ConsoleColor.DarkGray,
                    LogLevel.Info => ConsoleColor.Cyan,
                    LogLevel.Success => ConsoleColor.Green,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    LogLevel.Error => ConsoleColor.Red,
                    _ => originalColor
                };

                string prefix = level switch
                {
                    LogLevel.Debug => "[DBG]",
                    LogLevel.Info => "[INFO]",
                    LogLevel.Success => "[OK ]",
                    LogLevel.Warning => "[WARN]",
                    LogLevel.Error => "[FAIL]",
                    _ => "[LOG]"
                };

                Console.WriteLine($"{prefix} {message}");
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
    }
}

public class DelegateLogger : IObfuscationLogger
{
    private readonly Action<LogLevel, string> _logAction;

    public DelegateLogger(Action<LogLevel, string> logAction)
    {
        _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
    }

    public void Log(LogLevel level, string message)
    {
        _logAction(level, message);
    }
}
