using Aiursoft.DbTools;
using Aiursoft.DbTools.Sqlite;
using Aiursoft.GptGateway.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.GptGateway.Sqlite;

public class SqliteSupportedDb(bool allowCache, bool splitQuery) : SupportedDatabaseType<GptGatewayDbContext>
{
    public override string DbType => "Sqlite";

    public override IServiceCollection RegisterFunction(IServiceCollection services, string connectionString)
    {
        return services.AddAiurSqliteWithCache<SqliteContext>(
            connectionString,
            splitQuery: splitQuery,
            allowCache: allowCache);
    }

    public override GptGatewayDbContext ContextResolver(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<SqliteContext>();
    }
}