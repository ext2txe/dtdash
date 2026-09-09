using Microsoft.Data.Sqlite;

namespace Dtdash;

public sealed record StoredNote(long NoteId, DateTime CreatedAt, string? Tag, string Text, bool LinkedObject);

public sealed class NoteStore
{
    private const int SchemaVersion = 1;
    private readonly string _path;
    public bool UseSchema { get; }

    public NoteStore(string path, bool useSchema)
    {
        _path = path;
        UseSchema = useSchema;
        if (useSchema) SQLiteStore.ValidateOrCreate(path);
    }

    public IReadOnlyList<StoredNote> Load()
    {
        if (!UseSchema) return [];
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT n.note_id, n.created_at, t.name, n.text, n.linked_object FROM notes n LEFT JOIN tags t ON t.tag_id = n.tag_id ORDER BY n.created_at DESC, n.note_id DESC";
        using var reader = command.ExecuteReader();
        var result = new List<StoredNote>();
        while (reader.Read())
            result.Add(new StoredNote(reader.GetInt64(0), DateTime.Parse(reader.GetString(1), null, System.Globalization.DateTimeStyles.RoundtripKind), reader.IsDBNull(2) ? null : reader.GetString(2), reader.GetString(3), reader.GetInt64(4) != 0));
        return result;
    }

    public string? LoadLastTag()
    {
        if (!UseSchema) return null;
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT t.name FROM notes n JOIN tags t ON t.tag_id = n.tag_id ORDER BY n.created_at DESC, n.note_id DESC LIMIT 1";
        return command.ExecuteScalar() as string;
    }

    public void Add(string? tag, string text)
    {
        using var connection = Open();
        using var transaction = connection.BeginTransaction();
        long? tagId = null;
        if (!string.IsNullOrWhiteSpace(tag))
        {
            using var tagCommand = connection.CreateCommand();
            tagCommand.Transaction = transaction;
            tagCommand.CommandText = "INSERT INTO tags(name) VALUES ($name) ON CONFLICT(name) DO UPDATE SET name = excluded.name RETURNING tag_id";
            tagCommand.Parameters.AddWithValue("$name", tag!);
            tagId = (long)tagCommand.ExecuteScalar()!;
        }
        using var noteCommand = connection.CreateCommand();
        noteCommand.Transaction = transaction;
        noteCommand.CommandText = "INSERT INTO notes(created_at, tag_id, text, linked_object) VALUES ($created, $tag, $text, 0)";
        noteCommand.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("O"));
        noteCommand.Parameters.AddWithValue("$tag", (object?)tagId ?? DBNull.Value);
        noteCommand.Parameters.AddWithValue("$text", text);
        noteCommand.ExecuteNonQuery();
        transaction.Commit();
    }

    private SqliteConnection Open()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
        var connection = new SqliteConnection($"Data Source={_path}");
        connection.Open();
        connection.ExecuteNonQuery("PRAGMA foreign_keys = ON");
        return connection;
    }
}

public static class SQLiteStore
{
    private const int SchemaVersion = 1;
    public static void ValidateOrCreate(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        using var connection = new SqliteConnection($"Data Source={path}");
        connection.Open();
        connection.ExecuteNonQuery("PRAGMA foreign_keys = ON");
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name, sql FROM sqlite_master WHERE type='table' AND name IN ('schema_info','tags','notes')";
        var tables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        using var reader = command.ExecuteReader();
        while (reader.Read()) tables[reader.GetString(0)] = reader.IsDBNull(1) ? "" : reader.GetString(1);
        if (tables.Count == 0)
        {
            using var transaction = connection.BeginTransaction();
            foreach (var sql in SchemaSql) connection.ExecuteNonQuery(sql, transaction);
            transaction.Commit();
            return;
        }
        if (tables.Count != 3 || !tables.ContainsKey("schema_info") || !tables.ContainsKey("tags") || !tables.ContainsKey("notes"))
            throw new InvalidDataException("The SQLite file does not match the required dtdash schema.");
        using var version = connection.CreateCommand();
        version.CommandText = "SELECT value FROM schema_info WHERE key = 'version'";
        if ((version.ExecuteScalar() as string) != SchemaVersion.ToString())
            throw new InvalidDataException($"Unsupported dtdash SQLite schema version (expected {SchemaVersion}).");
    }

    private static readonly string[] SchemaSql =
    [
        "CREATE TABLE schema_info (key TEXT PRIMARY KEY, value TEXT NOT NULL)",
        $"INSERT INTO schema_info(key, value) VALUES ('version', '{SchemaVersion}')",
        "CREATE TABLE tags (tag_id INTEGER PRIMARY KEY, name TEXT NOT NULL COLLATE NOCASE UNIQUE)",
        "CREATE TABLE notes (note_id INTEGER PRIMARY KEY, created_at TEXT NOT NULL, tag_id INTEGER NULL, text TEXT NOT NULL, linked_object INTEGER NOT NULL DEFAULT 0 CHECK (linked_object IN (0, 1)), FOREIGN KEY (tag_id) REFERENCES tags(tag_id) ON UPDATE CASCADE ON DELETE SET NULL)",
        "CREATE INDEX idx_notes_created_at ON notes(created_at)",
        "CREATE INDEX idx_notes_tag_id ON notes(tag_id)"
    ];
}

file static class SqliteExtensions
{
    public static void ExecuteNonQuery(this SqliteConnection connection, string sql, SqliteTransaction? transaction = null)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Transaction = transaction;
        command.ExecuteNonQuery();
    }
}
