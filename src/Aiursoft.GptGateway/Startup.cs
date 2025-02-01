using Aiursoft.Canon;
using Aiursoft.DbTools.Sqlite;
using Aiursoft.GptGateway.Data;
using Aiursoft.GptGateway.Services;
using Aiursoft.GptGateway.Services.Abstractions;
using Aiursoft.GptGateway.Services.Plugins;
using Aiursoft.GptGateway.Services.PostRequest;
using Aiursoft.GptGateway.Services.PreRequest;
using Aiursoft.WebTools.Abstractions.Models;

namespace Aiursoft.GptGateway;

public class Startup : IWebStartup
{
    public virtual void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddAiurSqliteWithCache<GptGatewayDbContext>(connectionString);

        services.AddTaskCanon();
        services.AddHttpClient();
        services.AddTransient<QuestionReformatService>();
        services.AddTransient<OpenAiService>();
        services.AddTransient<SearchService>();
        
        services.AddScoped<IPreRequestMiddleware, TrimInputMiddleware>();
        services.AddScoped<IPreRequestMiddleware, InjectTimeMiddleware>();
        services.AddScoped<IPreRequestMiddleware, InjectPluginsMiddleware>();

        var searchKey = configuration["BingSearchAPIKey"];
        if (!string.IsNullOrWhiteSpace(searchKey))
        {
            services.AddScoped<IPlugin, SearchPlugin>();
        }

        //services.AddScoped<IPlugin, WikiPlugin>();
        services.AddScoped<IPostRequestMiddleware, RecordInDbMiddleware>();
        services.AddScoped<IPostRequestMiddleware, MockModelMiddleware>();
        services.AddScoped<IPostRequestMiddleware, ShowPluginUsageMiddleware>();

        services
            .AddControllers()
            .AddApplicationPart(typeof(Startup).Assembly);
    }

    public void Configure(WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.MapDefaultControllerRoute();
    }
}