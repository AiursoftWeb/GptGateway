using System.Text;
using System.Text.Json;
using Aiursoft.CSTools.Tools;
using Aiursoft.DbTools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptClient.Services;
using Aiursoft.GptGateway.Entities;
using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Tests.Services;
using Microsoft.Extensions.DependencyInjection;
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

    [TestMethod]
    public async Task TestThinkingModel()
    {
        var model = new OpenAiRequestModel
        {
            Messages = [new() { Role = "user", Content = "hi" }],
            Stream = false,
            Model = "test-thinking-model"
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpointUrl}/api/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", "Bearer test-api-key");
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var mockService = _server!.Services.GetRequiredService<ChatClient>() as MockOpenAiService;
        Assert.IsNotNull(mockService);
        var lastModel = mockService.LastModel as OllamaRequestModelOverride;
        Assert.IsNotNull(lastModel);
        Assert.IsTrue(lastModel.Think);
        Assert.AreEqual("ollama-thinking", lastModel.Model);
    }

    [TestMethod]
    public async Task TestOptionsModel()
    {
        var model = new OpenAiRequestModel
        {
            Messages = [new() { Role = "user", Content = "hi" }],
            Stream = false,
            Model = "test-options-model"
        };
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpointUrl}/api/chat")
        {
            Content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", "Bearer test-api-key");
        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var mockService = _server!.Services.GetRequiredService<ChatClient>() as MockOpenAiService;
        Assert.IsNotNull(mockService);
        var lastModel = mockService.LastModel as OllamaRequestModelOverride;
        Assert.IsNotNull(lastModel);
        Assert.IsNotNull(lastModel.Options);
        Assert.AreEqual(4096, lastModel.Options.NumCtx);
        Assert.AreEqual(0.1f, lastModel.Options.Temperature);
        Assert.AreEqual(0.2f, lastModel.Options.TopP);
        Assert.AreEqual(10, lastModel.Options.TopK);
        Assert.AreEqual("ollama-options", lastModel.Model);
    }
}
