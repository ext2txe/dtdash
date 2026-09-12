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
}
