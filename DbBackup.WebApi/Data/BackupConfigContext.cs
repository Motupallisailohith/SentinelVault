using Microsoft.EntityFrameworkCore;
using DbBackup.WebApi.Models;

namespace DbBackup.WebApi.Data;

public class BackupConfigContext : DbContext
{
    public BackupConfigContext(
        DbContextOptions<BackupConfigContext> opts) : base(opts) {}

    public DbSet<BackupProfile> BackupProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BackupProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.Engine).IsRequired();
            entity.Property(e => e.Host).IsRequired();
            entity.Property(e => e.Port).IsRequired();
            entity.Property(e => e.Username).IsRequired();
            entity.Property(e => e.Database).IsRequired();
            entity.Property(e => e.OutPath).IsRequired();
            entity.Property(e => e.Compression).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }

    /// <summary>
    /// Compares two string IDs
    /// </summary>
    public static bool CompareIds(string id1, string id2) => 
        string.Equals(id1, id2, StringComparison.OrdinalIgnoreCase);
}
