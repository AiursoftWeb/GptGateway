using Aiursoft.GptGateway.Api.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.GptGateway.Api.Data;

public class GptGatewayDbContext : DbContext
{
    public GptGatewayDbContext(DbContextOptions<GptGatewayDbContext> options) : base(options)
    {
    }
    
    public DbSet<UserConversation> UserConversations => Set<UserConversation>();
}