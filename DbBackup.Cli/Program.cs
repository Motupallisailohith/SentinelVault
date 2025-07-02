using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DbBackup.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Slack;
using DbBackup.Core;
using DbBackup.Adapters;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return 1;
        }

        // 1) build config + host
        var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

        var slackUrl = config["Slack:WebhookUrl"];
var loggerCfg = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.json", rollingInterval: RollingInterval.Day);
var manifestDir = Path.Combine(AppContext.BaseDirectory, "manifests");
Directory.CreateDirectory(manifestDir);   // make sure the folder exists
if (!string.IsNullOrWhiteSpace(slackUrl))
    loggerCfg = loggerCfg.WriteTo.Slack(slackUrl,
        restrictedToMinimumLevel: LogEventLevel.Information);

Log.Logger = loggerCfg.CreateLogger();

        using var host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((_, svc) =>
            {
                svc.AddSingleton<ICompressor, ZstdStreamCompressor>();
                svc.AddSingleton<IStorageProvider, LocalStorageProvider>();
                 svc.AddSingleton(new DbBackup.Core.JsonManifestStore(manifestDir));
                svc.AddSingleton<DbBackup.Core.IDatabaseAdapter, MySqlAdapter>();
                svc.AddSingleton<DbBackup.Core.IDatabaseAdapter, PostgresAdapter>();
                svc.AddSingleton<DbBackup.Core.IDatabaseAdapter, SqliteAdapter>();
                svc.AddSingleton<DbBackup.Core.IDatabaseAdapter, MongoDbAdapter>();
                svc.AddSingleton<BackupOrchestrator>();
            })
            .Build();

        await host.StartAsync();
        var orchestrator = host.Services.GetRequiredService<BackupOrchestrator>();

        // 2) parse
        var cmd = args[0].ToLowerInvariant();
        var opts = ParseOptions(args.Skip(1).ToArray());

        try
        {
            switch (cmd)
            {
                case "test-connection":
                    return await RunTestConnection(opts);

                case "backup":
                    return await RunBackup(orchestrator, opts);

                case "restore":
                    return await RunRestore(orchestrator, opts);

                default:
                    Console.Error.WriteLine($"Unknown command '{cmd}'");
                    ShowUsage();
                    return 1;
            }
        }
        finally
        {
            await host.StopAsync();
        }
    }

    static Dictionary<string,string> ParseOptions(string[] args)
    {
        var d = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < args.Length; i++)
        {
            var a = args[i];
            if (!a.StartsWith("--")) continue;
            var key = a.Substring(2);
            if (i+1 < args.Length && !args[i+1].StartsWith("--"))
            {
                d[key] = args[++i];
            }
            else
            {
                d[key] = "true";
            }
        }
        return d;
    }

    static void ShowUsage()
    {
        Console.WriteLine(@"
Usage:
  test-connection --engine <mysql|postgres> --host <h> --port <p> --user <u> --password <pw> --database <db>
  backup          --engine <mysql|postgres> [--type full|inc] --host <h> --port <p> --user <u> --password <pw> --database <db> --out <folder> [--s3 true]
  restore         --engine <mysql|postgres> --file <path> --host <h> --port <p> --user <u> --password <pw> --database <db>
");
    }

    static async Task<int> RunTestConnection(Dictionary<string,string> o)
    {
        if (!o.TryGetValue("engine", out var engine) ||
            !o.TryGetValue("host",   out var host)   ||
            !o.TryGetValue("user",   out var user)   ||
            !o.TryGetValue("password",out var pw)     ||
            !o.TryGetValue("database",out var db))
        {
            Console.Error.WriteLine("Missing required test-connection args");
            return 1;
        }

        int port = o.TryGetValue("port", out var ps) && int.TryParse(ps, out var p) ? p : (engine=="postgres" ? 5432 : 3306);

        bool ok = engine.Equals("mysql", StringComparison.OrdinalIgnoreCase)
            ? await MySqlAdapter.CanConnectAsync(host, port, user, pw, db)
            : await PostgresAdapter.CanConnectAsync(host, port, user, pw, db);

        Console.WriteLine(ok ? "✅ Connection successful" : "❌ Connection failed");
        return ok ? 0 : 1;
    }

    static async Task<int> RunBackup(BackupOrchestrator orchestrator, Dictionary<string,string> o)
    {
        if (!o.TryGetValue("engine", out var engine) ||
            !o.TryGetValue("host",   out var host)   ||
            !o.TryGetValue("user",   out var user)   ||
            !o.TryGetValue("password",out var pw)     ||
            !o.TryGetValue("database",out var db)     ||
            !o.TryGetValue("out",    out var outDir))
        {
            Console.Error.WriteLine("Missing required backup args");
            return 1;
        }

        var type = (o.TryGetValue("type", out var t) && t.Equals("inc", StringComparison.OrdinalIgnoreCase))
                   ? DbBackup.Core.BackupType.Incremental
                   : DbBackup.Core.BackupType.Full;

        int port = o.TryGetValue("port", out var ps) && int.TryParse(ps, out var p) ? p : 3306;
        bool useS3 = o.TryGetValue("s3", out var s) && bool.TryParse(s, out var b) && b;

        var req = new DbBackup.Core.BackupRequest(engine, db, host, port, user, pw, "", type);

        var result = await orchestrator.RunAsync(req, CancellationToken.None);
        if (result.Success)
        {
            Console.WriteLine($"✅ Backup saved");
            return 0;
        }
        else
        {
            Console.Error.WriteLine($"❌ Backup failed: {result.Error}");
            return 1;
        }
    }

    static async Task<int> RunRestore(BackupOrchestrator orchestrator, Dictionary<string,string> o)
    {
        if (!o.TryGetValue("engine", out var engine) ||
            !o.TryGetValue("file",   out var file)   ||
            !o.TryGetValue("host",   out var host)   ||
            !o.TryGetValue("user",   out var user)   ||
            !o.TryGetValue("password",out var pw)     ||
            !o.TryGetValue("database",out var db))
        {
            Console.Error.WriteLine("Missing required restore args");
            return 1;
        }

        int port = o.TryGetValue("port", out var ps) && int.TryParse(ps, out var p) ? p : 3306;

        var req = new RestoreRequest(
            Engine:     engine,
            Host:       host,
            Port:       port,
            User:       user,
            Password:   pw,
            Database:   db,
            SourcePath: file
        );

        await orchestrator.RestoreAsync(req, CancellationToken.None);
        Console.WriteLine("✅ Restore completed");
        return 0;
    }
}

