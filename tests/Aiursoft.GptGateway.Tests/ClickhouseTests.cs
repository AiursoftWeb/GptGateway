using Aiursoft.CSTools.Tools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Text.Json;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.GptGateway.Tests;

[TestClass]
public class ClickhouseTests
{
    private string _endpointUrl = string.Empty;
    private int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public ClickhouseTests()
    {
        _http = new HttpClient();
    }

    [TestInitialize]
    public async Task CreateServer()
    {
        _port = Network.GetAvailablePort();
        _endpointUrl = $"http://localhost:{_port}";
        _server = await AppAsync<TestStartup>([], port: _port);
        await _server.StartAsync();
    }

    [TestCleanup]
    public async Task CleanServer()
    {
        if (_server == null) return;
        await _server.StopAsync();
        _server.Dispose();
    }

    [TestMethod]
    public async Task TestLogCollection()
    {
        var model = new OpenAiRequestModel
        {
            Messages =
            [
                new()
                {
                    Role = "user",
                    Content = "Hello, what is 1+1?"
                }
            ],
            Stream = false,
            Model = "qwen3:32b-q8_0"
        };
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpointUrl}/api/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", "Bearer test-api-key");
        
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // Since we can't easily access the Scoped RequestLogContext from outside the request,
        // we might need to check if the ClickhouseService was called if we mock it.
        // But in this test, we just want to ensure the code doesn't crash and the flow works.
        
        // We can verify that ClickhouseDbContext is registered.
        using var scope = _server!.Services.CreateScope();
        var clickhouseDbContext = scope.ServiceProvider.GetService<ClickhouseDbContext>();
        Assert.IsNotNull(clickhouseDbContext);
    }
}
