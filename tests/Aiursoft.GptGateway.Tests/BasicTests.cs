using System.Text;
using System.Text.Json;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.GptGateway.Data;
using Aiursoft.GptGateway.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        await _server.UpdateDbAsync<GptGatewayDbContext>(UpdateMode.MigrateThenUse);
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
        var model = new OpenAiModel
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
            Model = "gpt-4",
            Temperature = 0.5,
            PresencePenalty = 0
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpointUrl}/v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode(); // Status Code 200-299
    }
}