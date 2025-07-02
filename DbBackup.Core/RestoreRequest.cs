// DbBackup.Core/RestoreRequest.cs
namespace DbBackup.Core;

/// <summary>
/// Parameters needed to restore a database from a full dump + optional
/// incremental binlogs.  Extend as you add S3, point-in-time, etc.
/// </summary>
public record RestoreRequest(
    string Engine,
    string Host,
    int    Port,
    string User,
    string Password,
    string Database,
    string SourcePath,          // local .zst file or s3:// key of the chosen backup
    DateTime? PointInTime = null);
