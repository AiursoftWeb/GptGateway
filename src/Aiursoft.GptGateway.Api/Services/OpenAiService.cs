using System.Text;
using System.Text.Json;
using Aiursoft.GptGateway.Api.Models;

namespace Aiursoft.GptGateway.Api.Services;

/// <summary>
/// https://platform.openai.com/docs/models/gpt-3-5
/// </summary>
public enum GptModel
{
    /// <summary>
    ///  gpt-3.5-turbo	Currently points to gpt-3.5-turbo-0613.	4,096 tokens	Up to Sep 2021
    /// </summary>
    Gpt35Turbo,

    /// <summary>
    /// gpt-3.5-turbo-16k	Currently points to gpt-3.5-turbo-16k-0613.	16,385 tokens	Up to Sep 2021
    /// </summary>
    Gpt35Turbo16K,
    
    /// <summary>
    /// gpt-4	Currently points to gpt-4-0613. See continuous model upgrades.	8,192 tokens	Up to Sep 2021
    /// </summary>
    Gpt4,
    
    /// <summary>
    /// gpt-4-32k	Currently points to gpt-4-32k-0613. See continuous model upgrades. This model was never rolled out widely in favor of GPT-4 Turbo.	32,768 tokens	Up to Sep 2021
    /// </summary>
    Gpt432K,
}

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
    
    public string ToModelString(GptModel gptModel)
    {
        return gptModel switch
        {
            GptModel.Gpt35Turbo => "gpt-3.5-turbo",
            GptModel.Gpt35Turbo16K => "gpt-3.5-turbo-16k",
            GptModel.Gpt4 => "gpt-4",
            GptModel.Gpt432K => "gpt-4-32k",
            _ => throw new ArgumentOutOfRangeException(nameof(gptModel), gptModel, null)
        };
    }

    public virtual async Task<CompletionData> AskModel(OpenAiModel model, GptModel gptModelType)
    {
        model.Model = ToModelString(gptModelType);
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

    public async Task<CompletionData> AskString(GptModel gptModelType, params string[] content)
    {
        var model = new OpenAiModel
        {
            Messages = content.Select(x => new MessagesItem
            {
                Content = x,
                Role = "user"
            }).ToList()
        };
        return await AskModel(model, gptModelType);
    }

    public virtual async Task<string> AskOne(string question, GptModel gptModelType)
    {
        var result = await AskString(gptModelType, question);
        return result.Choices.FirstOrDefault()?.Message?.Content ?? "No answer.";
    }
}