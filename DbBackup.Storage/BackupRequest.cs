namespace DbBackup.Storage;



public record BackupRequest(
    string Engine,
    string Host,
    int Port,
    string Username,
    string Password,
    string Database,
    DbBackup.Core.BackupType Type,
    string Compression = "zstd",
    string OutPath = "./backups")
{
    public BackupRequest() : this(
        Engine: "mysql",
        Host: "localhost",
        Port: 3306,
        Username: "",
        Password: "",
        Database: "",
        Type: DbBackup.Core.BackupType.Full) { }
}
