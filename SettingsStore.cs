using System.Text.Json;

namespace Dtdash;

public static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string GetPath(string configPath) =>
        Path.Combine(Path.GetDirectoryName(configPath)!, "settings.json");

    public static List<SettingEntry> Load(string configPath)
    {
        var path = GetPath(configPath);
        try
        {
            if (File.Exists(path))
            {
                var settings = JsonSerializer.Deserialize<List<SettingEntry>>(File.ReadAllText(path));
                if (settings is not null && settings.Count > 0) return settings;
            }
        }
        catch
        {
        }

        var defaults = new List<SettingEntry>
        {
            new(
                "PathToNotes",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "data", "notes.txt"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dtdash", "data", "notes.txt"),
                "Notes",
                1,
                "Location and name of the file used to save notes.")
        };
        Save(configPath, defaults);
        return defaults;
    }

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
