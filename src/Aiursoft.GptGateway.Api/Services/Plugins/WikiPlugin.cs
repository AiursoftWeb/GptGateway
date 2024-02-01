using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;
// ReSharper disable UnusedMember.Local
// ReSharper disable NotAccessedField.Local

namespace Aiursoft.GptGateway.Api.Services.Plugins;

public class WikiPlugin : IPlugin
{
    private readonly ILogger<WikiPlugin> _logger;
    public string PluginName => "维基百科插件";
    
    private const string ShouldUse =
        "你是一个旨在解决人类问题的人工智能。现在我遇到了一个问题\n\n```\n{0}\n```\n\n我无法阅读上面这个问题，但是对于正在探索一个名词概念的类型的问题，我可以访问维基百科。我需要你帮我判断一下这个问题是否值得检索一下维基百科。如果它有明确的正在研究的实体并且需要一些解释，请输出 `true`，否则请输出 `false`。不要输出其它内容。";

    public WikiPlugin(ILogger<WikiPlugin> logger)
    {
        _logger = logger;
    }

    public Task<int> GetUsagePoint(OpenAiModel input)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetPluginAppendedMessage(ConversationContext context)
    {
        throw new NotImplementedException();
    }
}