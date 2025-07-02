namespace DbBackup.Core;

public class BackupProfile
{
    public Guid   Id          { get; set; }
    public string Name        { get; set; } = default!;
    public string Description { get; set; } = string.Empty;

    // connection details
    public string Engine      { get; set; } = "mysql";
    public string Host        { get; set; } = "localhost";
    public int    Port        { get; set; } = 3306;
    public string Username    { get; set; } = "";
    public string Password    { get; set; } = "";
    public string Database    { get; set; } = "";

    public string OutPath     { get; set; } = "./backups";
    public string Compression { get; set; } = "zstd";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
