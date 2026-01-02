using Aiursoft.DbTools;
using Aiursoft.GptGateway.Entities;
using Aiursoft.GptGateway.Services;
using Aiursoft.WebTools;

namespace Aiursoft.GptGateway;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var app = await Extends.AppAsync<Startup>(args);
        await app.UpdateDbAsync<GptGatewayDbContext>();
        
        using (var scope = app.Services.CreateScope())
        {
            var clickhouseService = scope.ServiceProvider.GetRequiredService<ClickhouseService>();
            await clickhouseService.Init();
        }
        
        await app.RunAsync();
    }
}
