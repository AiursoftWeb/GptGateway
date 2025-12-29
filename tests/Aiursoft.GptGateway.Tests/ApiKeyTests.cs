using System.Net;
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
public class ApiKeyTests
{
    private readonly string _endpointUrl;
    private readonly int _port;
    private readonly HttpClient _http;
    private IHost? _server;
    private const string TestApiKey = "test-api-key";

    public ApiKeyTests()
    {
        _port = Network.GetAvailablePort();
        _endpointUrl = $"http://localhost:{_port}";
        _http = new HttpClient();
    }

    [TestInitialize]
    public async Task CreateServer()
    {
        // Pass ApiKey via command line arguments
        _server = await AppAsync<TestStartup>(["--ApiKey", TestApiKey], port: _port);
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
    public async Task TestUnauthorized()
    {
        var model = new OpenAiRequestModel
        {
            Messages = [new() { Role = "user", Content = "hi" }],
            Model = "qwen3:30b-a3b-thinking-2507-q8_0"
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpointUrl}/api/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        // No Authorization header
        var response = await _http.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task TestWrongKey()
    {
        var model = new OpenAiRequestModel
        {
            Messages = [new() { Role = "user", Content = "hi" }],
            Model = "qwen3:30b-a3b-thinking-2507-q8_0"
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpointUrl}/api/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", "Bearer wrong-key");
        var response = await _http.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task TestCorrectKey()
    {
        var model = new OpenAiRequestModel
        {
            Messages = [new() { Role = "user", Content = "hi" }],
            Model = "qwen3:30b-a3b-thinking-2507-q8_0"
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpointUrl}/api/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {TestApiKey}");
        var response = await _http.SendAsync(request);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task TestNoAuthNeededForOtherPaths()
    {
        // Version endpoint should also be protected if it starts with /api
        // But what if it's not?
        // Let's check a path that doesn't start with /api or /v1
        var response = await _http.GetAsync($"{_endpointUrl}/"); 
        // Should be 404 or something else, but NOT 401 if it's a static file or something
        Assert.AreNotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
