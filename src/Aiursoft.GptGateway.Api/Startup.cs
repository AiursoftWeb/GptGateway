using System.Reflection;
using Aiursoft.Canon;
using Aiursoft.DbTools.Sqlite;
using Aiursoft.GptGateway.Api.Data;
using Aiursoft.GptGateway.Api.Services;
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
        
        services.AddScoped<IPreRequestMiddleware, FixModelMiddleware>();
        services.AddScoped<IPostRequestMiddleware, RecordInDbMiddleware>();
        
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