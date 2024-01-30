using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.Plugins;

public class SearchPlugin : IPlugin
{
    public string PluginName => "搜索插件";
    
    private const string _shouldUse =
        "你是一个旨在解决人类问题的人工智能。现在我正在调查问题\n\n```\n{0}\n```\n\n面对某些问题，尤其是我们不了解的问题，我们可能需要使用搜索引擎，查找一些关键词汇，得到更多背景知识，再结合搜索的结果得出答案。\n\n现在，我需要你帮我判断，这个问题是否适合使用搜索引擎。如果适合，请输出 `true`，否则请输出 `false`。不要输出其它内容。";

    private const string _getSearchEntityPrompt =
        "你是一个旨在解决人类问题的人工智能。现在我正在调查问题\n\n```\n{0}\n```\n\n我计划先使用搜索引擎搜索一些背景。可是，我并不知道我应该搜索什么。请直接输出一个适合搜索引擎用于搜索的内容。不要输出其它内容。";

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
    
    public async Task<int> GetUsagePoint(string question)
    {
        var shouldSearch = await _openAiService.AskOne(string.Format(_shouldUse, question), GptModel.Gpt35Turbo);
        var truePosition = shouldSearch.IndexOf("true", StringComparison.Ordinal);
        var falsePosition = shouldSearch.IndexOf("false", StringComparison.Ordinal);
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

    public async Task<string> GetPluginAppendedMessage(string question, ConversationContext context)
    {
        var getSearchEntityPrompt = string.Format(_getSearchEntityPrompt, question);
        var textToSearch = (await _openAiService.AskOne(getSearchEntityPrompt, GptModel.Gpt4)).Trim('\"');
        context.UserMessages.Add($"""(使用了{PluginName}插件，搜索了"{textToSearch})"...""");
        
        var searchResult = await _searchService.DoSearch(textToSearch);
        var resultList = searchResult.WebPages?.Value
            .Select(t => $"""
                          ## {t.Name}

                          {t.Snippet}
                          
                          """).ToArray();
        var formattedResult = resultList?.Any() ?? false ? string.Join("\n", resultList) : "没有搜索到任何结果。";
        
        return string.Format(_answerPrompt, textToSearch, formattedResult, question);
    }
}