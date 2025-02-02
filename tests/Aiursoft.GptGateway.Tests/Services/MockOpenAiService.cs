using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptClient.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aiursoft.GptGateway.Tests.Services;

public class MockOpenAiService(
    HttpClient httpClient,
    ILogger<ChatClient> logger,
    IConfiguration configuration)
    : ChatClient(httpClient, logger, configuration)
{
    public override Task<CompletionData> AskModel(OpenAiModel model, GptModel gptModelType)
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

    public override Task<CompletionData> AskString(GptModel gptModelType, params string[] content)
    {
        var question = content[0];
        if (question.Contains("这个问题是否适合使用搜索引擎。如果适合，请输出 `true`，否则请输出 `false`。"))
        {
            var result = new CompletionData();
            result.SetContent("这个问题应该返回 true，而不是 false。");
            return Task.FromResult(result);
        }
        
        if (question.Contains("请告诉我我应该搜索的文字。不要输出其它内容。"))
        {
            var result = new CompletionData();
            result.SetContent("成年丝袜选购指南");
            return Task.FromResult(result);
        }
        
        return base.AskString(gptModelType, content);
    }
}