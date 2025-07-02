using System;
using DbBackup.WebApi.Models;

namespace DbBackup.WebApi.Data
{
    public static class TestProfile
    {
        public static async Task CreateTestProfileAsync(BackupConfigContext db)
        {
            var profile = new BackupProfile
            {
                Id = Guid.NewGuid(),
                Name = "Test Profile",
                Description = "A test backup profile",
                Engine = "mysql",
                Host = "localhost",
                Port = 3306,
                Username = "test",
                Password = "test",
                Database = "test_db",
                OutPath = "backups",
                Compression = "zstd",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await db.BackupProfiles.AddAsync(profile);
            await db.SaveChangesAsync();
        }
    }
}
