namespace Common.Interfaces;

/// <summary>
/// Interfaz de logger personalizada
/// </summary>
public interface ILogger
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception exception, string message, params object[] args);
}

/// <summary>
/// Implementación básica de logger
/// </summary>
public class ConsoleLogger : ILogger
{
    public void LogInformation(string message, params object[] args)
    {
        Console.WriteLine($"[INFO] {string.Format(message, args)}");
    }

    public void LogWarning(string message, params object[] args)
    {
        Console.WriteLine($"[WARN] {string.Format(message, args)}");
    }

    public void LogError(string message, params object[] args)
    {
        Console.WriteLine($"[ERROR] {string.Format(message, args)}");
    }

    public void LogError(Exception exception, string message, params object[] args)
    {
        Console.WriteLine($"[ERROR] {string.Format(message, args)} - Exception: {exception.Message}");
    }
}
