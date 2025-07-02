namespace DbBackup.Core;



public record BackupRequest(
    string Engine,
    string Database,
    string Host,
    int Port,
    string User,
    string Password,
    string ProfileId,
    BackupType Type)
{
    public BackupRequest() : this(
        Engine: "mysql",
        Database: "",
        Host: "localhost",
        Port: 3306,
        User: "",
        Password: "",
        ProfileId: "",
        Type: BackupType.Full)
    { }
}
