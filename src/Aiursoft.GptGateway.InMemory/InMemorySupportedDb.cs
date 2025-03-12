using Aiursoft.DbTools;
using Aiursoft.DbTools.InMemory;
using Aiursoft.GptGateway.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Aiursoft.GptGateway.InMemory;

public class InMemorySupportedDb : SupportedDatabaseType<GptGatewayDbContext>
{
    public override string DbType => "InMemory";

    public override IServiceCollection RegisterFunction(IServiceCollection services, string connectionString)
    {
        return services.AddAiurInMemoryDb<InMemoryContext>();
    }

    public override GptGatewayDbContext ContextResolver(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<InMemoryContext>();
    }
}