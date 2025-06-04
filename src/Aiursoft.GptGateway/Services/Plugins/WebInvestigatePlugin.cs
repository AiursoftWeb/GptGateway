using System.Text.RegularExpressions;
using Aiursoft.Canon;
using Aiursoft.CSTools.Tools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Services.Abstractions;
using Aiursoft.GptGateway.Services.Underlying;
using Microsoft.Azure.CognitiveServices.Search.WebSearch.Models;

namespace Aiursoft.GptGateway.Services.Plugins;

public class WebInvestigatePlugin(
    QuestionReformatService questionReformatService,
    RetryEngine retryEngine,
    CanonPool canonPool,
    SearchService searchService,
    ILogger<SearchPlugin> logger)
    : IPlugin
{
    public string PluginName => "web-investigate";

    private static readonly string[] BadSearchWords =
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

    public async Task ProcessMessage(ConversationContext context, IUnderlyingService service, CancellationToken cancellationToken)
    {
        var question = questionReformatService.MergeAsString(
            model: context.ModifiedInput,
            take: 5,
            out var _);

        // Get trimmed question.
        var trimmedQuestion = await GetTrimmedQuestion(question, service, context.ModifiedInput.Model!, cancellationToken);
        var initialSearch = await GetInitialSearchEntries(trimmedQuestion, service, context.ModifiedInput.Model!, cancellationToken);

        var searchedPages = new List<WebPage>();
        foreach (var searchTerm in initialSearch)
        {
            canonPool.RegisterNewTaskToPool(async () =>
            {
                var searchResult = await retryEngine.RunWithRetry(_ => searchService.DoSearch(searchTerm, 10, cancellationToken));
                var pages = searchResult.WebPages?.Value!;
                lock (searchedPages)
                {
                    searchedPages.AddRange(pages);
                }
            });
        }
        await canonPool.RunAllTasksInPoolAsync(maxDegreeOfParallelism: 3);
        var searchedText = searchedPages
            .GroupBy(t => t.DisplayUrl) // Distinct by URL
            .Select(t => t.First())
            .Select(t =>
                $"""
                  ## {t.Name}

                  {t.DisplayUrl}

                  {t.Snippet}

                  """).ToArray();
        var formattedResult = searchedText.Any() ? string.Join("\n\n", searchedText) : "没有搜索到任何结果。";

        // TODO: Ask AI is the result enough for investigation? Optional to additional search 2 more times.
        // TODO: Ask AI based on the result, which pages (TOP 15) shall we visit.
        // TODO: For each page:
        //   TODO: Actually visit the pages and get the content. Trim the HTML.
        //   TODO: Ask AI to find useful information from the trimmed HTML. Or any other link might provide helpful information.
        //   TODO: If (Need to visit additional pages) { Keep doing this loop until we have enough information. }
        // TODO: Summarize from all the information we have. Ask AI to summarize the information.

        const string answerPrompt =
            "你是一个旨在解决人类问题的人工智能。现在我正在调查一个问题。在调查之前，我简单搜索了 `{0}`。并得到了这样的搜索结果。\n\n\n{1}\n\n 这些结果或许有一些参考价值吧，也可能没有。就当是补充一些你的知识了。\n\n现在，问题是：\n\n```\n{2}\n```\n\n请解答。在给出答案时，如果可以，请在答案的最终附带使用 markdown 的链接格式 (也就是：[标题](URL) 的格式) 的引用部分来表达你引用的是哪条来源。";
        var finalPrompt = string.Format(
            answerPrompt,
            trimmedQuestion,
            formattedResult,
            question);
        context.ModifiedInput.Messages =
        [
            new MessagesItem
            {
                Role = "user",
                Content = finalPrompt,
            }
        ];
    }

    private async Task<string> GetTrimmedQuestion(string rawQuestion, IUnderlyingService service, string model, CancellationToken cancellationToken)
    {
        var trimmedQuestion = rawQuestion;
        while (true)
        {
            if (trimmedQuestion.Length < 300)
            {
                logger.LogInformation(
                    "The question user asked {RawQuestion} is too short (Length {Length}), we will try to expand it.",
                    trimmedQuestion.SafeSubstring(30), trimmedQuestion.Length);

                trimmedQuestion =
                    await retryEngine.RunWithRetry(
                        _ => ExpandQuestion(trimmedQuestion, service, model, cancellationToken),
                        attempts: 5);
            }
            else if (trimmedQuestion.Length > 2000)
            {
                logger.LogInformation(
                    "The question user asked {RawQuestion} is too long (Length {Length}), we will try to trim it.",
                    trimmedQuestion.SafeSubstring(30), trimmedQuestion.Length);

                trimmedQuestion =
                    await retryEngine.RunWithRetry(
                        _ => FormatQuestion(trimmedQuestion, service, model, cancellationToken),
                        attempts: 5);
            }
            else
            {
                logger.LogTrace(
                    "The question user asked {RawQuestion} is normal (Length {Length}), we will not change it.",
                    trimmedQuestion.SafeSubstring(30), rawQuestion.Length);
                break;
            }
        }
        return trimmedQuestion;
    }

    private async Task<string> ExpandQuestion(string rawQuestion, IUnderlyingService service, string model, CancellationToken cancellationToken)
    {
        const string expandPrompt =
            """
            我们公司的业务是：每天我们的客户会向我们询问大量的问题。为了解答这些问题，我们有一整套完整的流水线。

            其中，你扮演的角色是问题的收集人员。有些客户并不能特别清晰的表述他的问题。所以你的职责就是确定用户究竟想调查的是什么。

            用户提交上来的问题是：

            ======================
            {0}
            ======================

            为了调查上述问题，我需要你将这个问题进行标准化，变成一个易于后续流程调查的问题。你也可以将这个问题进一步扩展，例如：你可以预测解答上面问题前后，会产生哪些间接问题：

            * 如何确保客户的需求得到满足？
            * 如何验证一个解决方案是否合理？
            * 在解决客户的问题的过程中可能遇到哪些问题？
            * 解决方案是否可以持久的可行还是具有时效性？如何确保解决方案长周期可靠？
            * 相关的其他领域客户可能需要注意的方面？

            我并不需要你解决上面的问题。我需要你做的事情非常简单：将客户的问题展开，试着得到几个核心的点，分别质询。这样，我们可以方便后面的调查人员分组去进行调查。

            假如用户的问题是：“如何健身减脂？”，那么你需要思考，然后输出格式形如：

            <questions>
            客户的问题是“如何健身减脂？”。为了更好的回答客户的问题，我们需要思考一系列新问题：

            * 如何成功策划健身以减脂？
            * 如何确保一个减脂计划切实可行？
            * 如何验证减脂的效果？
            * 实施未来的减脂计划的过程中，可能遇到哪些问题？如何解决？
            * 我们的减脂计划是否是长周期的？如何确保它长周期可靠？
            * 在减脂过程中，是否需要注意什么别的，比如:
              * 如何在减脂期间作息以帮助减脂？
              * 在减脂期间应该如何饮食？
              * 穿着什么服装进行健身更好？
            * ... (上面只是例子。你可以添加、补充更多你可以补充的。至少800字。不要在输出里包含这行内容） ...
            </questions>

            假如用户的问题是：“如何让 Lunarlake CPU 在 Linux 下能够正确安装声卡驱动？”，那么你需要思考，然后输出格式形如：

            <questions>
            客户询问了“如何让 Lunarlake CPU 在 Linux 下能够正确安装声卡驱动？”。为了更好的回答客户的问题，我们需要思考一系列新问题：

            * 客户可能是在什么情境下遇到了问题？
            * Lunarlake 的 CPU 搭载了什么声卡？
            * 如何确保声卡驱动一定可以工作？
            * Linux 和 Lunarlake 有什么特殊之处？如何初步调查问题的根源？
            * 如何验证 Linux 里声卡正常工作？
            * 实施驱动安装的过程中，可能遇到哪些问题？如何解决？
            * 我们的解决方案是否是长周期的？如何确保随着未来更新，始终能得到正确的声卡驱动？
            * 在配置的过程中，是否需要注意什么别的，比如:
              * 错误的声卡驱动是否可能损坏硬件？
              * 如何确保声卡驱动来源的安全？
            * ... (上面只是例子。你可以添加、补充更多你可以补充的。至少800字。不要在输出里包含这行内容） ...
            </questions>

            我只是给了你两个例子。现在，你可以开展你的工作了。你需要你自己标准化的展开问题，得到一系列的不依赖上下文的question。（你不需要回答问题）我们会把你输出的内容交给其他调查人员去调查。

            请按照 <questions></questions> 的格式输出吧。输出5-15条即可。
            """;
        var result = await service.AskFormattedText(
            template: expandPrompt,
            content: rawQuestion,
            model: model,
            cancellationToken: cancellationToken);

        var regex = new Regex(@"<questions>([\s\S]*?)</questions>", RegexOptions.IgnoreCase);
        var match = regex.Match(result);
        if(match.Success)
        {
            var questionsContent = match.Groups[1].Value.Trim();
            return questionsContent;
        }
        else
        {
            throw new Exception($"Failed to extract questions from the request '{rawQuestion}'. response: '{result}'.");
        }
    }

    private async Task<string> FormatQuestion(string rawQuestion, IUnderlyingService service, string model, CancellationToken cancellationToken)
    {
        const string expandPrompt =
            """
            我遇到了下面的问题：

            ======================
            {0}
            ======================

            但是这个问题太冗长了，我需要你修剪它一下：在确保上下文不丢失的情况下，有条件的去掉一些无意义的信息，格式化文中的错别字，使用标准的markdown语法来输出。

            你输出的格式化后的文本，必须将内容放置在 <markdown></markdown> 之间。请确保你概括的问题内容中**包含必要的上下文信息**以方便后续调查人员调查。不要疏漏重要的信息。

            现在，请按照 <markdown></markdown> 的格式输出吧。
            """;
        var result = await service.AskFormattedText(
            template: expandPrompt,
            content: rawQuestion,
            model: model,
            cancellationToken: cancellationToken);

        var regex = new Regex(@"<markdown>([\s\S]*?)</markdown>", RegexOptions.IgnoreCase);
        var match = regex.Match(result);
        if(match.Success)
        {
            var questionsContent = match.Groups[1].Value.Trim();
            return questionsContent;
        }
        else
        {
            throw new Exception($"Failed to format markdown from the request '{rawQuestion}'. response: '{result}'.");
        }
    }

    private async Task<string[]> GetInitialSearchEntries(string rawQuestion, IUnderlyingService service, string model, CancellationToken cancellationToken)
    {
        const string getSearchEntityPrompt =
            """
            现在我正在调查问题

            ```markdown
            {0}
            ```

            我计划先使用搜索引擎搜索一些背景。可是，我并不知道我应该搜索什么。现在你需要扮演一位搜索引擎的使用专家，请指导我填写一些适合搜索引擎用于搜索的词条。

            使用搜索引擎有一些最佳实践，例如你要注意使用文字精准的描述前面的上下文，而不是搜索 '那里' '当时' '当地' 这种搜索引擎无法理解的上下文，注意合理的分词而不是搜索句子。

            不要包含解释性文字。

            现在，请输出我们需要在搜索引擎的搜索框里填的搜索内容吧！你输出的格式必须使用 <search></search> 包围起来的内容。最多输出5个。给出1-4个为宜。

            """;
        var result = await service.AskFormattedText(
            template: getSearchEntityPrompt,
            content: rawQuestion,
            model: model,
            cancellationToken: cancellationToken);
        var textToSearch = BadSearchWords
            .Aggregate(result, (current, badWord) => current.Replace(badWord, " "))
            .Trim();

        var regex = new Regex(@"<search>(.*?)<\/search>", RegexOptions.Singleline);
        var searchTerms = regex.Matches(textToSearch)
            .Select(m => m.Groups[1].Value.Trim())
            .ToArray();

        if (searchTerms.Length == 0)
        {
            throw new Exception($"Failed to extract search terms from the request '{rawQuestion}'. response: '{result}'.");
        }
        else
        {
            logger.LogInformation("Search plugin needs to search: {Count} terms. First term: {FirstTerm}. We only take the first 5 terms.",
                searchTerms.Length, searchTerms[0].SafeSubstring(30));
            return searchTerms.Take(5).ToArray();
        }
    }
}
