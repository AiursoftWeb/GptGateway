using Aiursoft.Canon;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools.Switchable;
using Aiursoft.GptClient;
using Aiursoft.GptGateway.InMemory;
using Aiursoft.GptGateway.Middlewares;
using Aiursoft.GptGateway.Models.Configuration;
using Aiursoft.GptGateway.MySql;
using Aiursoft.GptGateway.Services;
using Aiursoft.GptGateway.Services.Abstractions;
using Aiursoft.GptGateway.Services.Plugins;
using Aiursoft.GptGateway.Services.PostRequest;
using Aiursoft.GptGateway.Services.PreRequest;
using Aiursoft.GptGateway.Services.Underlying;
using Aiursoft.GptGateway.Sqlite;
using Aiursoft.WebTools.Abstractions.Models;

namespace Aiursoft.GptGateway;

public class Startup : IWebStartup
{
    public virtual void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
    {
        // Relational database
        var (connectionString, dbType, allowCache) = configuration.GetDbSettings();
        services.AddSwitchableRelationalDatabase(
            dbType: EntryExtends.IsInUnitTests() ? "InMemory": dbType,
            connectionString: connectionString,
            supportedDbs:
            [
                new MySqlSupportedDb(allowCache: allowCache, splitQuery: false),
                new SqliteSupportedDb(allowCache: allowCache, splitQuery: true),
                new InMemorySupportedDb()
            ]);

        // Configuration
        services.Configure<UnderlyingsOptions>(configuration.GetSection("Underlyings"));
        services.Configure<GptModelOptions>(options =>
        {
            options.DefaultIncomingModel = configuration["DefaultIncomingModel"]!;
            options.SupportedModels = configuration.GetSection("SupportedModels")
                .Get<List<SupportedModel>>()!;
        });

        services.AddTaskCanon();
        services.AddHttpClient();
        services.AddGptClient();
        services.AddScoped<StreamTransformService>();
        services.AddTransient<QuestionReformatService>();
        services.AddTransient<SearchService>();

        // TODO: Use these plugins.
        services.AddScoped<IPreRequestMiddleware, TrimInputMiddleware>();
        services.AddScoped<IPreRequestMiddleware, InjectTimeMiddleware>();

        var searchKey = configuration["BingSearchAPIKey"];
        if (!string.IsNullOrWhiteSpace(searchKey))
        {
            Console.WriteLine("Search plugin enabled. Search key: " + searchKey.SafeSubstring(15));
            services.AddScoped<IPlugin, SearchPlugin>();
            services.AddScoped<IPlugin, WebInvestigatePlugin>();
        }

        services.AddScoped<IPostRequestMiddleware, MockModelMiddleware>();

        services.AddScoped<IUnderlyingService, OpenAiService>();
        services.AddScoped<IUnderlyingService, DeepSeekService>();
        services.AddScoped<IUnderlyingService, OllamaService>();

        services
            .AddControllers()
            .AddApplicationPart(typeof(Startup).Assembly);
    }

    public void Configure(WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseRequestLogging();
        app.MapDefaultControllerRoute();
    }
}
