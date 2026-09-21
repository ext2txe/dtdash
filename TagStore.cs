using System.Text.Json;

namespace Dtdash;

public static class TagStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static List<string> Load(string path)
    {
        try
        {
            if (File.Exists(path))
                return (JsonSerializer.Deserialize<List<string>>(File.ReadAllText(path)) ?? []).Take(10).ToList();
        }
        catch { }
        return [];
    }

    public static void Add(string path, string tag)
    {
        tag = tag.Trim();
        if (string.IsNullOrWhiteSpace(tag)) return;
        var tags = Load(path);
        tags.RemoveAll(existing => string.Equals(existing, tag, StringComparison.OrdinalIgnoreCase));
        tags.Insert(0, tag);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(tags.Take(10).ToList(), JsonOptions));
    }

    public static bool Remove(string path, string tag)
    {
        try
        {
            var tags = Load(path);
            if (tags.RemoveAll(existing => string.Equals(existing, tag, StringComparison.OrdinalIgnoreCase)) == 0)
                return false;

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(tags.Take(10).ToList(), JsonOptions));
            return true;
        }
        catch { return false; }
    }

    public static HashSet<string> LoadUsedTags(string notesPath)
    {
        var usedTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            if (!File.Exists(notesPath)) return usedTags;

            foreach (var line in File.ReadLines(notesPath))
            {
                var note = line.TrimStart();
                if (note.StartsWith("- ", StringComparison.Ordinal))
                    note = note[2..];

                var firstSeparator = note.IndexOf(" - ", StringComparison.Ordinal);
                if (firstSeparator < 0) continue;
                var secondSeparator = note.IndexOf(" - ", firstSeparator + 3, StringComparison.Ordinal);
                if (secondSeparator <= firstSeparator) continue;

                var tag = note[(firstSeparator + 3)..secondSeparator].Trim();
                if (!string.IsNullOrWhiteSpace(tag))
                    usedTags.Add(tag);
            }
        }
        catch { }

        return usedTags;
    }
}
