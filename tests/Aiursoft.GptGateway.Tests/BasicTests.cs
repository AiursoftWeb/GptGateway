using System.Text;
using System.Text.Json;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Entities;
using Microsoft.Extensions.Hosting;
using static Aiursoft.WebTools.Extends;

[assembly:DoNotParallelize]

namespace Aiursoft.GptGateway.Tests;

[TestClass]
public class BasicTests
{
    private string _endpointUrl = string.Empty;
    private int _port;
    private readonly HttpClient _http;
    private IHost? _server;

    public BasicTests()
    {
        _http = new HttpClient();
    }

    [TestInitialize]
    public async Task CreateServer()
    {
        _port = Network.GetAvailablePort();
        _endpointUrl = $"http://localhost:{_port}";
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
            Model = "qwen3:32b-q8_0",
            Temperature = 0.5,
            PresencePenalty = 0
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpointUrl}/api/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", "Bearer test-api-key");
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode(); // Status Code 200-299
    }
}
