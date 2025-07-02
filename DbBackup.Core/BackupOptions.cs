// DbBackup.Core/BackupOptions.cs
namespace DbBackup.Core
{
    public class BackupOptions
    {
        /// <summary>
        /// Root folder under which each profile's backups live
        /// </summary>
        public string LocalOutPath { get; set; } = "backups";
    }
}


