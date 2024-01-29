using Aiursoft.WebTools;

namespace Aiursoft.GptGateway;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var app = Extends.App<Startup>(args);
        await app.RunAsync();
    }
}