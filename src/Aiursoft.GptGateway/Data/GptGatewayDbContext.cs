using Aiursoft.GptGateway.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.GptGateway.Data;

public class GptGatewayDbContext(DbContextOptions<GptGatewayDbContext> options) : DbContext(options)
{
    public DbSet<UserConversation> UserConversations => Set<UserConversation>();
    
    public DbSet<OpenAiRequest> OpenAiRequests => Set<OpenAiRequest>();
}