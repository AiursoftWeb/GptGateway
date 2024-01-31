using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.Plugins;

public class SearchPlugin : IPlugin
{
    public string PluginName => "搜索插件";
    
    private const string _shouldUse =
        "你是一个旨在解决人类问题的人工智能。现在我遇到了一个问题\n\n```\n{0}\n```\n\n虽然你的知识是固定的，但是面对一些需要实时性信息和关于一些可能变化的对象，我可以帮助你使用搜索引擎，查找一些关键词汇来得到更多背景知识，再结合搜索的结果得出答案。\n\n现在，我需要你帮我判断，这个问题是否含有一些不了解的或需要实时信息的实体，使得我们应该搜索引擎来解决。如果适合，请输出 `true`，否则请输出 `false`。不要输出其它内容。";

    private const string _getSearchEntityPrompt =
        "你是一个旨在解决人类问题的人工智能。现在我正在调查问题\n\n```\n{0}\n```\n\n我计划先使用搜索引擎搜索一些背景。可是，我并不知道我应该搜索什么。请直接输出一个适合搜索引擎用于搜索的词条。不要输出其它内容。";

    private const string _answerPrompt =
        "你是一个旨在解决人类问题的人工智能。现在我正在调查一个问题。在调查之前，我使用搜索引擎，搜索了 `{0}`。下面是Bing搜索的结果。\n\n\n{1}\n\n 现在，结合上述搜索结果，回答真正调查的问题：\n\n```\n{2}\n```";
    
    private readonly SearchService _searchService;
    private readonly OpenAiService _openAiService;

    public SearchPlugin(
        SearchService searchService,
        OpenAiService openAiService)
    {
        _searchService = searchService;
        _openAiService = openAiService;
    }
    
    public async Task<int> GetUsagePoint(OpenAiModel model)
    {
        var messages = model.Messages
            .Where(m => m.Role == "user")
            .TakeLast(4)
            .ToArray();
        var finalQuestion = model.Messages.LastOrDefault()?.Content;
        var formattedFinalQuestion = string.Format(_shouldUse, finalQuestion);
        messages[^1] = new MessagesItem
        {
            Role = "user",
            Content = formattedFinalQuestion,
        };
        var requestModel = new OpenAiModel
        {
            Messages = messages.ToList(),
        };

        var shouldSearch = await _openAiService.AskModel(requestModel, GptModel.Gpt432K);
        var shouldSearchOutput = shouldSearch.Choices.FirstOrDefault()!.Message!.Content!;
        var truePosition = shouldSearchOutput.IndexOf("true", StringComparison.Ordinal);
        var falsePosition = shouldSearchOutput.IndexOf("false", StringComparison.Ordinal);
        if (truePosition == -1)
        {
            return 0;
        }
        if (falsePosition == -1)
        {
            return 60;
        }
        return truePosition < falsePosition ? 60 : 0;
    }

    public async Task<string> GetPluginAppendedMessage(OpenAiModel model, ConversationContext context)
    {
        var messages = model.Messages
            .Where(m => m.Role == "user")
            .TakeLast(4)
            .ToArray();
        var finalQuestion = model.Messages.LastOrDefault()?.Content;
        var formattedFinalQuestion = string.Format(_getSearchEntityPrompt, finalQuestion);
        messages[^1] = new MessagesItem
        {
            Role = "user",
            Content = formattedFinalQuestion,
        };
        var requestModel = new OpenAiModel
        {
            Messages = messages.ToList(),
        };

        var textToSearchObject = await _openAiService.AskModel(requestModel, GptModel.Gpt4);
        var textToSearch = textToSearchObject.Choices.FirstOrDefault()!.Message!.Content!.Trim('\"').Trim();
        context.UserMessages.Add($@"> 使用搜索引擎搜索了：""{textToSearch}"".");
        
        var searchResult = await _searchService.DoSearch(textToSearch);
        var resultList = searchResult.WebPages?.Value
            .Select(t => $"""
                          ## {t.Name}

                          {t.Snippet}
                          
                          """).ToArray();
        var formattedResult = resultList?.Any() ?? false ? string.Join("\n", resultList) : "没有搜索到任何结果。";
        
        return string.Format(_answerPrompt, textToSearch, formattedResult, finalQuestion);
    }
}