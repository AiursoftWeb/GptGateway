using System.Text;
using System.Text.Json;

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Aiursoft.GptGateway.Api.Services;

public class OpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string _token;
    private readonly string _instance;

    public OpenAiService(
        HttpClient httpClient,
        ILogger<OpenAiService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
        _logger = logger;
        _token = configuration["OpenAI:Token"]!;
        _instance = configuration["OpenAI:Instance"]!;
    }
    
    public virtual async Task<CompletionData> Ask(OpenAiModel model)
    {
        if (string.IsNullOrWhiteSpace(_token))
        {
            throw new ArgumentNullException(nameof(_token));
        }

        _logger.LogInformation("Asking OpenAi...");


        var json = JsonSerializer.Serialize(model);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{_instance}/v1/chat/completions")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Authorization", $"Bearer {_token}");
        var response = await _httpClient.SendAsync(request);
        try
        {
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseModel = JsonSerializer.Deserialize<CompletionData>(responseJson);
            return responseModel!;
        }
        catch (HttpRequestException raw)
        {
            var remoteError = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(remoteError, raw);
        }
    }

    public async Task<CompletionData> Ask(params string[] content)
    {
        var model = new OpenAiModel
        {
            Messages = content.Select(x => new MessagesItem
            {
                Content = x,
                Role = "user"
            }).ToList()
        };
        return await Ask(model);
    }
}