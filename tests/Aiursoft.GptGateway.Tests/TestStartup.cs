using Aiursoft.GptClient.Services;
using Aiursoft.GptGateway.Services;
using Aiursoft.GptGateway.Tests.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aiursoft.GptGateway.Tests;

public class TestStartup : Startup
{
    public override void ConfigureServices(IConfiguration configuration, IWebHostEnvironment environment, IServiceCollection services)
    {
        base.ConfigureServices(configuration, environment, services);
        services.RemoveAll<ChatClient>();
        services.AddTransient<ChatClient, MockOpenAiService>();
        
        services.RemoveAll<SearchService>();
        services.AddTransient<SearchService, MockSearchService>();
    }
}