using System.Text.Json;

namespace Dtdash;

public static class ConfigStore
{
    public static string ResolvePath(string[] args)
    {
        var supplied = args.FirstOrDefault(a => !string.IsNullOrWhiteSpace(a) && !a.StartsWith('-'));
        var path = supplied is null
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "config.json")
            : Path.GetFullPath(supplied);
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(new AvBotConfiguration(), new JsonSerializerOptions { WriteIndented = true }));
        }
        return path;
    }

    // Initial placeholder for the named AvBot/AvAutomation settings; expand after the PoC.
    public sealed class AvBotConfiguration
    {
        public string Name { get; set; } = "dtdash";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 8080;
        public bool Enabled { get; set; } = true;
    }
}
