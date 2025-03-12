using Aiursoft.GptGateway.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aiursoft.GptGateway.Sqlite;

public class SqliteContext(DbContextOptions<SqliteContext> options) : GptGatewayDbContext(options)
{
    public override Task<bool> CanConnectAsync()
    {
        return Task.FromResult(true);
    }
}
