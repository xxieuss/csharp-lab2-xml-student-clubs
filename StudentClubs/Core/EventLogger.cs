namespace Core;

public sealed class EventLogger {
    private static readonly Lazy<EventLogger> _instance = new Lazy<EventLogger>(() => new EventLogger());

    private readonly string _logPath;
    private readonly object _sync = new object();

    public static EventLogger Instance => _instance.Value;

    public string LogPath => _logPath;

    private EventLogger() {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty(root)) root = AppContext.BaseDirectory;

        _logPath = Path.Combine(root, "StudentClubs", "events.log");
    }

    public void Log(LogLevel level, string message) => Append(new LogEntry(DateTime.Now, level, message));

    private void Append(LogEntry entry) {
        lock (_sync) {
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
                File.AppendAllText(_logPath, entry + Environment.NewLine);
            } catch (IOException) {
                // a failed log record must not break searching or saving
            } catch (UnauthorizedAccessException) {
                // the same
            }
        }
    }
}