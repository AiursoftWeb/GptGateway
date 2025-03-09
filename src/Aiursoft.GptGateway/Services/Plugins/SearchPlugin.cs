using Aiursoft.Canon;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptClient.Services;
using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Services.Abstractions;

namespace Aiursoft.GptGateway.Services.Plugins;

public class SearchPlugin(
    QuestionReformatService questionReformatService,
    RetryEngine retryEngine,
    ILogger<SearchPlugin> logger,
    SearchService searchService,
    IConfiguration configuration,
    ChatClient openAiService)
    : IPlugin
{
    public string PluginName => "搜索插件";
    private readonly GptModel _model = Enum.Parse<GptModel>(configuration["OpenAI:Model"]!);

    private static readonly string[] BadWords =
    [
        "今天", "昨天", "明天",
        "今日", "昨日", "明日",
        "现在", "当下", "目前",
        "这里", "那里",
        "这个", "那个",
        "这种", "那种",
        "这样", "那样",
        "这么", "那么",
        "这些", "那些",
        "当时", "当地",
        "当天", "当年", "当月", "当周",
        "如何", "什么",
        "哪里", "哪个", "哪种", "哪样", "哪么", "哪些",
        "是什么", "是哪里", "是哪个", "是哪种", "是哪样", "是哪么", "是哪些",
        "怎么", "怎么样", "怎么着", "怎么办", "怎么会", "怎么说", "怎么做", "怎么了",
        "建议", "搜索", "词条", "\""
    ];

    private const string ShouldUse =
        "你是一个旨在解决人类问题的人工智能。现在我遇到了一个问题\n\n```\n{0}\n```\n\n虽然你的知识是固定的，但是面对一些需要实时性信息和关于一些可能变化的对象，我们可以使用搜索引擎查找一些关键词汇来得到更多背景知识，再结合搜索的结果得出答案。\n\n当然，如果只是问候、翻译等，这类不需要借助搜索引擎的工作，我们可以正常回答。\n\n现在，我需要你帮我判断，为了解决这个问题，是否必须用搜索引擎来调查一些实体的知识，也就是超出了你的理解范围。如果应当，请输出 `true`，否则请输出 `false`。不要输出其它内容。";

    private const string GetSearchEntityPrompt =
        "你是一个旨在解决人类问题的人工智能。现在我正在调查问题\n\n```\n{0}\n```\n\n我计划先使用搜索引擎搜索一些背景。可是，我并不知道我应该搜索什么。现在你需要扮演一位搜索引擎的使用专家，请直接输出一个适合搜索引擎用于搜索的词条，注意使用文字精准的描述前面的上下文而不是搜索 '那里' '当时' '当地' 这种搜索引擎无法理解的上下文，注意合理的分词而不是搜索句子。不要在搜索的内容中包含日期。不要输出其它内容和解释性文字，而是只输出我需要输入给搜索引擎的词条。";

    private const string AnswerPrompt =
        "你是一个旨在解决人类问题的人工智能。现在我正在调查一个问题。在调查之前，我简单搜索了 `{0}`。并得到了这样的搜索结果。\n\n\n{1}\n\n 这些结果或许有一些参考价值吧，也可能没有。就当是补充一些你的知识了。\n\n现在，问题是：\n\n```\n{2}\n```\n\n请解答。在给出答案时，如果可以，请在答案的最终附带使用 markdown 的链接格式 (也就是：[标题](URL) 的格式) 的引用部分来表达你引用的是哪条来源。";

    public async Task<int> GetUsagePoint(OpenAiModel input)
    {
        var requestModel = questionReformatService.Map(
            input,
            ShouldUse,
            4,
            false,
            out var _);

        var longRequest = string.Join(',', requestModel.Messages.Select(m => m.Content)).Length > 800;
        if (longRequest)
        {
            return 0;
        }

        var shouldSearch = await openAiService.AskModel(requestModel, _model);

        return questionReformatService.ConvertResponseToScore(shouldSearch);
    }

    public async Task<string> GetPluginAppendedMessage(ConversationContext context)
    {
        var requestModel = questionReformatService.Map(
            model: context.ModifiedInput,
            template: GetSearchEntityPrompt,
            take: 5,
            includeSystemMessage: true,
            out var rawQuestion,
            mergeAsOne: true);
        var textToSearchObject = await openAiService.AskModel(requestModel, _model);
        var textToSearch = textToSearchObject.GetAnswerPart()
            .Trim('\"')
            .Trim();

        textToSearch = BadWords.Aggregate(textToSearch, (current, badWord) => current.Replace(badWord, " ")).Trim();

        logger.LogInformation("Search plugin needs to search: {0}", textToSearch);

        context.PluginMessages.Add($@"> 使用搜索引擎搜索了：""{textToSearch}"".");

        var searchResult = await retryEngine.RunWithRetry(_ => searchService.DoSearch(textToSearch));
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
