using Aiursoft.CSTools.Tools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Extensions;
using Aiursoft.GptGateway.Models.Configuration;
using ClickHouse.Client.ADO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
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
        await _server.InitClickhouseAsync();
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
        var options = _server!.Services.GetRequiredService<IOptions<ClickhouseOptions>>();
        if (!options.Value.Enabled)
        {
            Assert.Inconclusive("Clickhouse is not enabled in appsettings.json for tests.");
            return;
        }

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

        // Wait a bit for Clickhouse to flush (though our SaveChanges is immediate)
        await Task.Delay(500);

        // Verify data in Clickhouse
        await using var connection = new ClickHouseConnection(options.Value.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT count() FROM RequestLogs WHERE LastQuestion = 'Hello, what is 1+1?'";
        var count = Convert.ToInt64(await command.ExecuteScalarAsync());
        
        Assert.IsTrue(count >= 1, "Should find at least one log entry in Clickhouse.");

        // Check columns
        command.CommandText = "SELECT Method, Path, StatusCode FROM RequestLogs WHERE LastQuestion = 'Hello, what is 1+1?' LIMIT 1";
        await using var reader = await command.ExecuteReaderAsync();
        Assert.IsTrue(await reader.ReadAsync());
        Assert.AreEqual("POST", reader["Method"].ToString());
        Assert.AreEqual("/api/chat", reader["Path"].ToString());
        Assert.AreEqual("200", reader["StatusCode"].ToString());
    }
}
