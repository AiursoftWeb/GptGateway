using Aiursoft.DbTools;
using Aiursoft.DbTools.MySql;
using Aiursoft.GptGateway.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.GptGateway.MySql;

public class MySqlSupportedDb(bool allowCache, bool splitQuery) : SupportedDatabaseType<GptGatewayDbContext>
{
    public override string DbType => "MySql";

    public override IServiceCollection RegisterFunction(IServiceCollection services, string connectionString)
    {
        return services.AddAiurMySqlWithCache<MySqlContext>(
            connectionString,
            splitQuery: splitQuery,
            allowCache: allowCache);
    }

    public override GptGatewayDbContext ContextResolver(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<MySqlContext>();
    }
}