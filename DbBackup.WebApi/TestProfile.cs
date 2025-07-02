using DbBackup.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using DbBackup.WebApi.Data;

namespace DbBackup.WebApi
{
    public static class TestProfile
    {
        public static async Task CreateTestProfileAsync(BackupConfigContext db)
        {
            var testProfile = new BackupProfile
            {
                Name = "Test Profile",
                Description = "Test database backup profile",
                Engine = "sqlite",  // Using SQLite for testing since it's easy to set up
                Host = "localhost",
                Port = 0,  // SQLite doesn't need a port
                Username = "",
                Password = "",
                Database = "test_db",
                OutPath = "./backups",
                Compression = "zstd"
            };

            await db.BackupProfiles.AddAsync(testProfile);
            await db.SaveChangesAsync();
        }
    }
}
