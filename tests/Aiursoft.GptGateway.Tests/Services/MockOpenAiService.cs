using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptClient.Services;
using Microsoft.Extensions.Logging;

namespace Aiursoft.GptGateway.Tests.Services;

public class MockOpenAiService(
    HttpClient httpClient,
    ILogger<ChatClient> logger)
    : ChatClient(httpClient, logger)
{
    public override Task<CompletionData> AskModel(OpenAiModel model, string completionApiUrl, string? token, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CompletionData
        {
            Id = "chat-id",
            Object = "chat.completion",
            Created = 1706527910,
            Model = "gpt-3.5-turbo-0301",
            Choices =
            [
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
            ],
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

    public override Task<HttpResponseMessage> AskStream(OpenAiModel model, string completionApiUrl, string? token, CancellationToken cancellationToken)
    {
        return Task.FromResult(new HttpResponseMessage());
    }

    public override Task<CompletionData> AskString(string modelType, string completionApiUrl, string? token, string[] content, CancellationToken cancellationToken)
    {
        return Task.FromResult(new CompletionData
        {
            Id = "chat-id",
            Object = "chat.completion",
            Created = 1706527910,
            Model = "gpt-3.5-turbo-0301",
            Choices =
            [
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
            ],
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
