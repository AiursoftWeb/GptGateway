using Aiursoft.DbTools;
using Aiursoft.GptGateway.Data;
using Aiursoft.WebTools;

namespace Aiursoft.GptGateway;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var app = await Extends.AppAsync<Startup>(args);
        await app.UpdateDbAsync<GptGatewayDbContext>(UpdateMode.MigrateThenUse);
        await app.RunAsync();
    }
}