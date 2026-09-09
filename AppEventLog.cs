namespace Dtdash;

public static class AppEventLog
{
    public static string GetLogPath(string configPath)
    {
        var configDirectory = Path.GetDirectoryName(configPath)!;
        var parentDirectory = Directory.GetParent(configDirectory)?.FullName ?? configDirectory;
        return Path.Combine(parentDirectory, "logs", $"{DateTime.Now:yyyyMMdd}_dtdash.log");
    }

    public static string GetSettingsPath(string configPath) =>
        SettingsStore.GetPath(configPath);

    public static string GetSettingsGeometryPath(string configPath) =>
        Path.Combine(Path.GetDirectoryName(configPath)!, "settings-window.json");

    public static void WriteStartup(string configPath)
    {
        var logPath = GetLogPath(configPath);
        Write(logPath, false,
            $"Application started (version {GetVersion()})",
            $"Executable: {Environment.ProcessPath ?? AppContext.BaseDirectory}",
            $"Current log: {logPath}",
            $"Configuration: {configPath}",
            $"Settings: {GetSettingsPath(configPath)}",
            $"Settings geometry: {GetSettingsGeometryPath(configPath)}");
    }

    public static void WriteShutdown(string configPath) =>
        Write(GetLogPath(configPath), true, $"Application shut down (version {GetVersion()})");

    public static void WriteGeometryIssue(string configPath, string message) =>
        Write(GetLogPath(configPath), false, message);

    private static string GetVersion() =>
        typeof(App).Assembly.GetName().Version?.ToString(3) ?? "unknown";

    private static void Write(string logPath, bool sessionSeparator, params string[] entries)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            var lines = entries.Select(entry => $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {entry}");
            File.AppendAllLines(logPath, lines);
            if (sessionSeparator)
                File.AppendAllText(logPath, Environment.NewLine);
        }
        catch
        {
            // Logging must not prevent the application from starting or shutting down.
        }
    }
}
