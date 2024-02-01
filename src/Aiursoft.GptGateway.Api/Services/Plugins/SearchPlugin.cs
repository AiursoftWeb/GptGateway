using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services.Abstractions;

namespace Aiursoft.GptGateway.Api.Services.Plugins;

public class SearchPlugin : IPlugin
{
    public string PluginName => "搜索插件";
    
    private const string ShouldUse =
        "你是一个旨在解决人类问题的人工智能。现在我遇到了一个问题\n\n```\n{0}\n```\n\n虽然你的知识是固定的，但是面对一些需要实时性信息和关于一些复杂的、可能变化的对象，我们可以使用搜索引擎查找一些关键词汇来得到更多背景知识，再结合搜索的结果得出答案。\n\n现在，我需要你帮我判断，为了解决这个问题，是否需要先用搜索引擎来调查一些实体的知识。如果应当，请输出 `true`，否则请输出 `false`。不要输出其它内容。";

    private const string GetSearchEntityPrompt =
        "你是一个旨在解决人类问题的人工智能。现在我正在调查问题\n\n```\n{0}\n```\n\n我计划先使用搜索引擎搜索一些背景。可是，我并不知道我应该搜索什么。现在你需要扮演一位搜索引擎的使用专家，请直接输出一个适合搜索引擎用于搜索的词条，注意使用文字精准的描述前面的上下文而不是搜索 '那里' '当时' '当地' 这种搜索引擎无法理解的上下文，注意合理的分词而不是句子。不要输出其它内容。";

    private const string AnswerPrompt =
        "你是一个旨在解决人类问题的人工智能。现在我正在调查一个问题。在调查之前，我使用搜索引擎，搜索了 `{0}`。下面是搜索的结果。\n\n\n{1}\n\n 可是，我无法阅读上述结果。现在，我需要你结合上述搜索结果，引用、推理、总结并回答真正正在调查的问题：\n\n```\n{2}\n```\n\n另外，在给出答案时，如果可以，请使用 markdown 的链接格式附带你引用的是哪条内容。";

    private readonly QuestionReformatService _questionReformatService;
    private readonly ILogger<SearchPlugin> _logger;
    private readonly SearchService _searchService;
    private readonly OpenAiService _openAiService;

    public SearchPlugin(
        QuestionReformatService questionReformatService,
        ILogger<SearchPlugin> logger,
        SearchService searchService,
        OpenAiService openAiService)
    {
        _questionReformatService = questionReformatService;
        _logger = logger;
        _searchService = searchService;
        _openAiService = openAiService;
    }
    
    public async Task<int> GetUsagePoint(OpenAiModel input)
    {
        var requestModel = _questionReformatService.Map(
            input,
            ShouldUse,
            4,
            false,
            out var _);

        var shouldSearch = await _openAiService.AskModel(requestModel, GptModel.Gpt432K);
        
        return _questionReformatService.ConvertResponseToScore(shouldSearch);
    }

    public async Task<string> GetPluginAppendedMessage(ConversationContext context)
    {
        var requestModel = _questionReformatService.Map(
            model: context.ModifiedInput,
            template: GetSearchEntityPrompt, 
            take: 12,
            includeSystemMessage: true,
            out var rawQuestion);
        var textToSearchObject = await _openAiService.AskModel(requestModel, GptModel.Gpt4);
        var textToSearch = textToSearchObject.Choices.FirstOrDefault()!.Message!.Content!.Trim('\"').Trim();
        _logger.LogInformation("Search plugin needs to search: {0}", textToSearch);
        
        context.PluginMessages.Add($@"> 使用搜索引擎搜索了：""{textToSearch}"".");
        
        var searchResult = await _searchService.DoSearch(textToSearch);
        var resultList = searchResult.WebPages?.Value
            .Select(t => $"""
                          ## {t.Name}

                          {t.DisplayUrl}
                          
                          {t.Snippet}
                          
                          """).ToArray();
        var formattedResult = resultList?.Any() ?? false ? string.Join("\n", resultList) : "没有搜索到任何结果。";
        
        return string.Format(AnswerPrompt, textToSearch, formattedResult, rawQuestion);
    }
}