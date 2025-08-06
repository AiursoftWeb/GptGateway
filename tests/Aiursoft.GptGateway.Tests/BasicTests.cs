using System.Text;
using System.Text.Json;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Entities;
using Microsoft.Extensions.Hosting;
using static Aiursoft.WebTools.Extends;

namespace Aiursoft.GptGateway.Tests;

[TestClass]
public class BasicTests
{
    private readonly string _endpointUrl;
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public BasicTests()
    {
        _port = Network.GetAvailablePort();
        _endpointUrl = $"http://localhost:{_port}";
        _http = new HttpClient();
    }

    [TestInitialize]
    public async Task CreateServer()
    {
        _server = await AppAsync<TestStartup>([], port: _port);
        await _server.UpdateDbAsync<GptGatewayDbContext>();
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
    public async Task PostApi()
    {
        var model = new OpenAiRequestModel
        {
            Messages =
            [
                new()
                {
                    Role = "user",
                    Content = "hi"
                }
            ],
            Stream = true,
            Model = "qwen3:30b-a3b-thinking-2507-q8_0",
            Temperature = 0.5,
            PresencePenalty = 0
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpointUrl}/api/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode(); // Status Code 200-299
    }
}
