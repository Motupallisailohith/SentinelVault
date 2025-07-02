namespace DbBackup.Core;

/// <summary>
/// Convenience helpers for <see cref="BackupManifest"/>.
/// </summary>
public static class BackupManifestExtensions
{
    /// <returns>
    /// The filename stem used by JsonManifestStore:
    ///   &lt;db&gt;_yyyyMMdd_HHmmss_full|incremental
    /// </returns>
    public static string UtcTimestampFileNameStem(this BackupManifest m)
        => $"{m.Database}_{m.UtcTimestamp:yyyyMMdd_HHmmss}_{m.Type.ToString().ToLower()}";
}
