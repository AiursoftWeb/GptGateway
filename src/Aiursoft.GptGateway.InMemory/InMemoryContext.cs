using Aiursoft.GptGateway.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.GptGateway.InMemory;

public class InMemoryContext(DbContextOptions<InMemoryContext> options) : GptGatewayDbContext(options)
{
    public override async Task MigrateAsync(CancellationToken cancellationToken)
    {
        await Database.EnsureDeletedAsync(cancellationToken);
        await Database.EnsureCreatedAsync(cancellationToken);
    }

    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}