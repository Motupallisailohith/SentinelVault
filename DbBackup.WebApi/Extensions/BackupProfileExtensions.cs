using DbBackup.Core;
using DbBackup.WebApi.Models;

namespace DbBackup.WebApi.Extensions;

public static class BackupProfileExtensions
{
    public static DbBackup.Core.BackupProfile ToCoreBackupProfile(this DbBackup.WebApi.Models.BackupProfile profile)
    {
        return new DbBackup.Core.BackupProfile
        {
            Id = profile.Id,
            Name = profile.Name,
            Description = profile.Description,
            Engine = profile.Engine,
            Host = profile.Host,
            Port = profile.Port,
            Username = profile.Username,
            Password = profile.Password,
            Database = profile.Database,
            OutPath = profile.OutPath,
            Compression = profile.Compression,
           
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
