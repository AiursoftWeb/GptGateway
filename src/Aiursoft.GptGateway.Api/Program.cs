using Aiursoft.DbTools;
using Aiursoft.GptGateway.Api.Data;
using Aiursoft.WebTools;

namespace Aiursoft.GptGateway.Api;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var app = Extends.App<Startup>(args);
        await app.UpdateDbAsync<GptGatewayDbContext>(UpdateMode.MigrateThenUse);
        await app.RunAsync();
    }
}