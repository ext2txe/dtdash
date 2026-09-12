using System.Text.Json;

namespace Dtdash;

public static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string GetPath(string configPath) =>
        Path.Combine(Path.GetDirectoryName(configPath)!, "settings.json");

    public static string ResolveActiveNotesPath(string configPath, DateTime date)
    {
        var settings = Load(configPath);
        var standardPath = settings.First(s => s.Name == "PathToNotes").Value;
        var obsidianEnabled = bool.TryParse(
            settings.FirstOrDefault(s => s.Name == "Obsidian Enabled")?.Value,
            out var enabled) && enabled;
        if (!obsidianEnabled)
            return standardPath;

        var folder = settings.FirstOrDefault(s => s.Name == "Obsidian Quick Notes Folder")?.Value
            ?? "__INBOX.Quick Notes";
        var vaultFolder = Path.IsPathRooted(folder)
            ? folder
            : Path.Combine(Path.GetDirectoryName(standardPath)!, folder);
        return Path.Combine(vaultFolder, $"{date:yyyy-MM-dd}.md");
    }

    public static bool IsObsidianEnabled(string configPath) =>
        bool.TryParse(Load(configPath).FirstOrDefault(s => s.Name == "Obsidian Enabled")?.Value, out var enabled) && enabled;

    public static List<SettingEntry> Load(string configPath)
    {
        var path = GetPath(configPath);
        try
        {
            if (File.Exists(path))
            {
                var settings = JsonSerializer.Deserialize<List<SettingEntry>>(File.ReadAllText(path));
                if (settings is not null && settings.Count > 0)
                {
                    var changed = false;
                    foreach (var defaultSetting in DefaultSettings())
                    {
                        if (settings.All(s => s.Name != defaultSetting.Name))
                        {
                            settings.Add(defaultSetting);
                            changed = true;
                        }
                    }

                    if (changed)
                        Save(configPath, settings);
                    return settings;
                }
            }
        }
        catch
        {
        }

        var defaults = DefaultSettings();
        Save(configPath, defaults);
        return defaults;
    }

    private static List<SettingEntry> DefaultSettings() =>
    [
        new(
            "PathToNotes",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "data", "notes.txt"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "data", "notes.txt"),
            "Notes",
            1,
            "Location and name of the file used to save notes."),
        new(
            "StartMinimized",
            "true",
            "true",
            "Application",
            1,
            "Start dtdash minimized.")
        ,new(
            "Sticky Quicke Edit window",
            "false",
            "false",
            "Application",
            2,
            "Keep the quick edit window open after saving a note.")
        ,new(
            "Obsidian Enabled",
            "false",
            "false",
            "Application",
            3,
            "Enable storing quick notes in Obsidian daily note files.")
        ,new(
            "Obsidian Quick Notes Folder",
            "__INBOX.Quick Notes",
            "__INBOX.Quick Notes",
            "Application",
            4,
            "Folder name for Obsidian quick notes within the configured vault.")
        ,new(
            "Keep on Top",
            "true",
            "true",
            "Application",
            5,
            "Keep the quick edit window above other windows.")
    ];

    public static void Save(string configPath, IEnumerable<SettingEntry> settings)
    {
        var path = GetPath(configPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(settings, JsonOptions));
    }

    public sealed class SettingEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public string Section { get; set; }
        public int SectionIndex { get; set; }
        public string Description { get; set; }

        public SettingEntry(string name, string value, string defaultValue, string section, int sectionIndex, string description)
        {
            Name = name;
            Value = value;
            DefaultValue = defaultValue;
            Section = section;
            SectionIndex = sectionIndex;
            Description = description;
        }

        public SettingEntry() : this(string.Empty, string.Empty, string.Empty, string.Empty, 0, string.Empty) { }
    }
}
