using System.Text;
using System.Text.Json;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.GptGateway.Api.Data;
using Aiursoft.GptGateway.Api.Models;
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
        _server = App<TestStartup>(Array.Empty<string>(), port: _port);
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
    [DataRow("/")]
    public async Task GetHome(string url)
    {
        var response = await _http.GetAsync(_endpointUrl + url);
        response.EnsureSuccessStatusCode(); // Status Code 200-299
    }

    [TestMethod]
    public async Task PostApi()
    {
        var model = new OpenAiModel
        {
            Messages = new List<MessagesItem>()
            {
                new MessagesItem()
                {
                    Role = "user",
                    Content = "hi"
                }
            },
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