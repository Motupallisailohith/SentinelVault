// DbBackup.Core/JsonManifestStore.cs
using System.Text.Json;

namespace DbBackup.Core;

/// <summary>
/// Persists <see cref="BackupManifest"/> records as one JSON file per backup and
/// provides lookup helpers needed for incremental-restore.
/// </summary>
public sealed class JsonManifestStore
{
    private readonly string _dir;
    private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

    public JsonManifestStore(string dir) => _dir = dir;

    /*────────────────────────  WRITE  ────────────────────────*/

    public void Save(BackupManifest m)
    {
        Directory.CreateDirectory(_dir);
        string file = Path.Combine(
            _dir,
            $"{m.Database}_{m.UtcTimestamp:yyyyMMdd_HHmmss}_{m.Type.ToString().ToLower()}.manifest.json");

        File.WriteAllText(file, JsonSerializer.Serialize(m, _opts));
    }

    /*────────────────────────  READ  ─────────────────────────*/

    /// <summary>Returns the most-recent *full* backup for <paramref name="db"/>.</summary>
    public BackupManifest? GetLatestFull(string db)
    {
        return Enumerate(db)
            .Where(x => x.Manifest.Type == BackupType.Full)
            .OrderByDescending(x => x.Manifest.UtcTimestamp)
            .Select(x => x.Manifest)
            .FirstOrDefault();
    }

    /// <summary>
    /// Returns manifest that belongs to the given .zst file, or <c>null</c>.
    /// </summary>
    public BackupManifest? FromFileName(string filePath)
    {
        string manifestPath = Path.ChangeExtension(filePath, ".manifest.json");
        return File.Exists(manifestPath)
            ? JsonSerializer.Deserialize<BackupManifest>(File.ReadAllText(manifestPath), _opts)
            : null;
    }

    /// <summary>
    /// Incrementals whose [BinlogStart, BinlogEnd] range lies within
    /// <paramref name="fromPos"/>..\<paramref name="toPos"/>.
    /// Returned in chronological order.
    /// </summary>
    public IEnumerable<(string File, BackupManifest Manifest)> GetIncrementals(
        string db,
        long fromPos,
        long toPos)
    {
        return Enumerate(db)
            .Where(x => x.Manifest.Type == BackupType.Incremental &&
                        x.Manifest.BinlogStart >= fromPos &&
                        x.Manifest.BinlogEnd   <= toPos)
            .OrderBy(x => x.Manifest.UtcTimestamp);
    }

    /*────────────────────  internal helper  ──────────────────*/

    private IEnumerable<(string File, BackupManifest Manifest)> Enumerate(string db)
    {
        Directory.CreateDirectory(_dir);
        foreach (string path in Directory.EnumerateFiles(_dir, $"{db}_*.manifest.json"))
        {
            BackupManifest? m = JsonSerializer.Deserialize<BackupManifest>(File.ReadAllText(path), _opts);
            if (m is not null)
            {
                string zst = Path.ChangeExtension(path, ".zst");  // same stem
                yield return (zst, m);
            }
        }
    }
}
