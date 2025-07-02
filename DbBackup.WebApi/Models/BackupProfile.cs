namespace DbBackup.WebApi.Models;

using DbBackup.Core;

public class BackupProfile : DbBackup.Core.BackupProfile
{
    public string IdString => Id.ToString();

    public BackupProfile()
    {
        Id = Guid.NewGuid();
        Name = string.Empty;
        Description = string.Empty;
        Engine = "mysql";
        Host = "localhost";
        Port = 3306;
        Username = string.Empty;
        Password = string.Empty;
        Database = string.Empty;
        OutPath = "./backups";
        Compression = "zstd";
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
