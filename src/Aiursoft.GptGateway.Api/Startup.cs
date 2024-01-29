using System.Reflection;
using Aiursoft.GptGateway.Api.Services;
using Aiursoft.WebTools.Abstractions.Models;

namespace Aiursoft.GptGateway.Api;

public class Startup : IWebStartup
{
    public void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddTransient<OpenAiService>();
        
        services
            .AddControllers()
            .AddApplicationPart(Assembly.GetExecutingAssembly());
    }

    public void Configure(WebApplication app)
    {
        app.UseRouting();
        app.MapDefaultControllerRoute();
    }
}