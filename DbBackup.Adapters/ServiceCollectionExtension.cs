using Microsoft.Extensions.DependencyInjection;
using DbBackup.Core;

namespace DbBackup.Adapters;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdapters(this IServiceCollection svc)
        => svc.AddSingleton<DbBackup.Core.IDatabaseAdapter, MySqlAdapter>()
            // Postgres adapter (register under a named key or switch in orchestrator)
            .AddSingleton<DbBackup.Core.IDatabaseAdapter, PostgresAdapter>()
            .AddSingleton<DbBackup.Core.IDatabaseAdapter, SqliteAdapter>()
            .AddSingleton<DbBackup.Core.IDatabaseAdapter, MongoDbAdapter>();
}
