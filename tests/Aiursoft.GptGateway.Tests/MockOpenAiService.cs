using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aiursoft.GptGateway.Tests;

public class MockOpenAiService : OpenAiService
{
    public MockOpenAiService(HttpClient httpClient, ILogger<OpenAiService> logger, IConfiguration configuration) 
        : base(httpClient, logger, configuration)
    {
    }

    public override Task<CompletionData> Ask(OpenAiModel model)
    {
        return Task.FromResult(new CompletionData
        {
            Id = "chat-id",
            Object = "chat.completion",
            Created = 1706527910,
            Model = "gpt-3.5-turbo-0301",
            Choices = new List<ChoicesItemData>
            {
                new()
                {
                    Index = 0,
                    Message = new MessageData
                    {
                        Role = "assistant",
                        Content = "I apologize, that was a mistake. 1 + 2 equals 3."
                    },
                    FinishReason = "stop"
                }
            },
            Usage = new UsageData
            {
                PromptTokens = 41,
                CompletionTokens = 17,
                TotalTokens = 58,
                PreTokenCount = 4096,
                PreTotal = 42,
                AdjustTotal = 41,
                FinalTotal = 1
            }
        });
    }
}