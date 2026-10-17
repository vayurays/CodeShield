using System;

namespace CodeShieldCore.Logging;

public class ObfuscationLogEventArgs : EventArgs
{
    public LogLevel Level { get; }
    public string Message { get; }
    public DateTime Timestamp { get; }

    public ObfuscationLogEventArgs(LogLevel level, string message)
    {
        Level = level;
        Message = message;
        Timestamp = DateTime.Now;
    }
}

public interface IObfuscationLogger
{
    void Log(LogLevel level, string message);
    void Debug(string message) => Log(LogLevel.Debug, message);
    void Info(string message) => Log(LogLevel.Info, message);
    void Success(string message) => Log(LogLevel.Success, message);
    void Warning(string message) => Log(LogLevel.Warning, message);
    void Error(string message) => Log(LogLevel.Error, message);
}
