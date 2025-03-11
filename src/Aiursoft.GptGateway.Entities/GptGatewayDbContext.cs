using Aiursoft.DbTools;
using Aiursoft.GptGateway.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.GptGateway.Entities;

public abstract class GptGatewayDbContext(DbContextOptions options) : DbContext(options), ICanMigrate
{
    public DbSet<UserConversation> UserConversations => Set<UserConversation>();

    public virtual  Task MigrateAsync(CancellationToken cancellationToken) =>
        Database.MigrateAsync(cancellationToken);

    public virtual  Task<bool> CanConnectAsync() =>
        Database.CanConnectAsync();
}
