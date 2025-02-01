using Aiursoft.Canon;
using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Models.OpenAi;
using Aiursoft.GptGateway.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Aiursoft.GptGateway.Tests.Services;

public class MockOpenAiService(
    CanonQueue queue,
    HttpClient httpClient,
    ILogger<OpenAiService> logger,
    IConfiguration configuration)
    : OpenAiService(queue, httpClient, logger, configuration)
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

    public override Task<string> AskOne(string question, GptModel model)
    {
        if (question.Contains("这个问题是否适合使用搜索引擎。如果适合，请输出 `true`，否则请输出 `false`。"))
        {
            return Task.FromResult("这个问题应该返回 true，而不是 false。");
        }

        if (question.Contains("请告诉我我应该搜索的文字。不要输出其它内容。"))
        {
            return Task.FromResult("成年丝袜选购指南");
        }
        return base.AskOne(question, model);
    }
}