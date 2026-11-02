using System.Globalization;

namespace Core;

public class LogEntry {
    public DateTime Time { get; }
    public LogLevel Level { get; }
    public string Message { get; }

    public LogEntry(DateTime time, LogLevel level, string message) {
        Time = time;
        Level = level;
        Message = message;
    }

    public override string ToString() =>
        $"{Time.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture)} {LevelName(Level)}. {Message}";

    private static string LevelName(LogLevel level) => level switch {
        LogLevel.Filtering => "Фільтрація",
        LogLevel.Transformation => "Трансформація",
        LogLevel.Saving => "Збереження",
        _ => level.ToString()
    };
}