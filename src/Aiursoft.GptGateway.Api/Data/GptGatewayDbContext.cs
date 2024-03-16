using Aiursoft.GptGateway.Api.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.GptGateway.Api.Data;

public class GptGatewayDbContext(DbContextOptions<GptGatewayDbContext> options) : DbContext(options)
{
    public DbSet<UserConversation> UserConversations => Set<UserConversation>();
    
    public DbSet<OpenAiRequest> OpenAiRequests => Set<OpenAiRequest>();
}