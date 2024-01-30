using System.Reflection;
using Aiursoft.Canon;
using Aiursoft.DbTools.Sqlite;
using Aiursoft.GptGateway.Api.Data;
using Aiursoft.GptGateway.Api.Services;
using Aiursoft.GptGateway.Api.Services.Abstractions;
using Aiursoft.GptGateway.Api.Services.Plugins;
using Aiursoft.GptGateway.Api.Services.PostRequest;
using Aiursoft.GptGateway.Api.Services.PreRequest;
using Aiursoft.WebTools.Abstractions.Models;

namespace Aiursoft.GptGateway.Api;

public class Startup : IWebStartup
{
    public virtual void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddAiurSqliteWithCache<GptGatewayDbContext>(connectionString);

        services.AddTaskCanon();
        services.AddHttpClient();
        services.AddTransient<OpenAiService>();
        services.AddTransient<SearchService>();
        
        services.AddScoped<IPreRequestMiddleware, FixModelMiddleware>();
        services.AddScoped<IPreRequestMiddleware, InjectTimeMiddleware>();
        services.AddScoped<IPreRequestMiddleware, InjectPluginsMiddleware>();
        services.AddScoped<IPlugin, SearchPlugin>();
        services.AddScoped<IPostRequestMiddleware, RecordInDbMiddleware>();
        services.AddScoped<IPostRequestMiddleware, MockModelMiddleware>();
        services.AddScoped<IPostRequestMiddleware, ShowPluginUsageMiddleware>();
        
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