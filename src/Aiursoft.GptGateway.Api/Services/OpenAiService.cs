using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Aiursoft.Canon;
using Aiursoft.CSTools.Tools;
using Aiursoft.GptGateway.Api.Data;
using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Models.Database;
using Aiursoft.GptGateway.Api.Models.OpenAi;

namespace Aiursoft.GptGateway.Api.Services;

public class OpenAiService
{
    private readonly CanonQueue _canonQueue;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly string _token;
    private readonly string _completionApiUrl;

    public OpenAiService(
        CanonQueue canonQueue,
        HttpClient httpClient,
        ILogger<OpenAiService> logger,
        IConfiguration configuration)
    {
        _canonQueue = canonQueue;
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
        _logger = logger;
        _token = configuration["OpenAI:Token"]!;
        _completionApiUrl = configuration["OpenAI:CompletionApiUrl"]!;
    }
    
    public string ToModelString(GptModel gptModel)
    {
        return gptModel switch
        {
            GptModel.Gpt35Turbo => "gpt-3.5-turbo",
            GptModel.Gpt35Turbo16K => "gpt-3.5-turbo-16k",
            GptModel.Gpt4 => "gpt-4",
            GptModel.Gpt432K => "gpt-4-32k",
            GptModel.DeepseekR132B => "deepseek-r1:32b",
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
        _logger.LogInformation("Asking OpenAi with endpoint: {0}.", _completionApiUrl);
        var request = new HttpRequestMessage(HttpMethod.Post, _completionApiUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Authorization", $"Bearer {_token}");
        var requestStartTimestamp = DateTime.UtcNow;
        var response = await _httpClient.SendAsync(request);
        try
        {
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseModel = JsonSerializer.Deserialize<CompletionData>(responseJson);
            
            _canonQueue.QueueWithDependency<GptGatewayDbContext>(db => RecordInDb(db, model, responseModel!, requestStartTimestamp));
            
            _logger.LogInformation("Asked OpenAi. Request last question: {0}. Response last answer: {1}.",
                model.Messages.LastOrDefault()?.Content?.SafeSubstring(30),
                responseModel?.Choices.FirstOrDefault()?.Message?.Content?.SafeSubstring(30));
            return responseModel!;
        }
        catch (HttpRequestException raw)
        {
            var remoteError = await response.Content.ReadAsStringAsync();
            
            _logger.LogError("Asked OpenAi failed. Request last question: {0}. Response last answer: {1}.",
                model.Messages.LastOrDefault()?.Content?.SafeSubstring(30),
                remoteError.SafeSubstring(30));
            throw new HttpRequestException(remoteError, raw);
        }
    }
    
    private async Task RecordInDb(GptGatewayDbContext db, OpenAiModel model, CompletionData responseModel, DateTime requestStartTimestamp)
    {
        var openAiRequest = new OpenAiRequest
        {
            LastQuestion = model.Messages.LastOrDefault()?.Content ?? "No question.",
            Questions = JsonSerializer.Serialize(model.Messages, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }),
            Answer = responseModel.Choices.FirstOrDefault()?.Message?.Content ?? "No answer.",
            Duration = DateTime.UtcNow - requestStartTimestamp,
            ConversationTime = DateTime.UtcNow,
            PromptTokens = responseModel.Usage?.PromptTokens ?? 0,
            CompletionTokens = responseModel.Usage?.CompletionTokens ?? 0,
            TotalTokens = responseModel.Usage?.TotalTokens ?? 0,
            PreTokenCount = responseModel.Usage?.PreTokenCount ?? 0,
            PreTotal = responseModel.Usage?.PreTotal ?? 0,
            AdjustTotal = responseModel.Usage?.AdjustTotal ?? 0,
            FinalTotal = responseModel.Usage?.FinalTotal ?? 0,
        };
        await db.OpenAiRequests.AddAsync(openAiRequest);
        await db.SaveChangesAsync();
        
        _logger.LogInformation("Recorded a new OpenAi request.");
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