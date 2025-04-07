using System.Text.RegularExpressions;
using Aiursoft.Canon;
using Aiursoft.CSTools.Tools;
using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Services.Abstractions;
using Aiursoft.GptGateway.Services.Underlying;

namespace Aiursoft.GptGateway.Services.Plugins;

public class WebInvestigatePlugin(
    QuestionReformatService questionReformatService,
    RetryEngine retryEngine,
    ILogger<SearchPlugin> logger,
    SearchService searchService)
    : IPlugin
{
    public string PluginName => "web-investigate";

    public async Task ProcessMessage(ConversationContext context, IUnderlyingService service)
    {
        var question = questionReformatService.MergeAsString(
            model: context.ModifiedInput,
            take: 5,
            out var rawQuestion);

        var trimmedQuestion = rawQuestion;
        while (true)
        {
            if (trimmedQuestion.Length < 450)
            {
                logger.LogInformation(
                    "The question user asked {RawQuestion} is too short (Length {Length}), we will try to expand it.",
                    trimmedQuestion.SafeSubstring(30), trimmedQuestion.Length);

                trimmedQuestion =
                    await retryEngine.RunWithRetry(
                        _ => ExpandQuestion(trimmedQuestion, service, context.ModifiedInput.Model!),
                        attempts: 5);
            }
            else if (trimmedQuestion.Length > 2000)
            {
                logger.LogInformation(
                    "The question user asked {RawQuestion} is too long (Length {Length}), we will try to trim it.",
                    trimmedQuestion.SafeSubstring(30), trimmedQuestion.Length);

                trimmedQuestion =
                    await retryEngine.RunWithRetry(
                        _ => TrimQuestion(trimmedQuestion, service, context.ModifiedInput.Model!),
                        attempts: 5);
            }
            else
            {
                logger.LogTrace(
                    "The question user asked {RawQuestion} is normal (Length {Length}), we will not change it.",
                    trimmedQuestion.SafeSubstring(30), question.Length);
                break;
            }
        }

        // TODO: Call search engine to get the search result.

        context.ModifiedInput.Messages =
        [
            new MessagesItem
            {
                Role = "user",
                Content = trimmedQuestion,
            }
        ];
    }

    private async Task<string> ExpandQuestion(string rawQuestion, IUnderlyingService service, string model)
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

            请按照 <questions></questions> 的格式输出吧。至少 800 字。
            """;
        var result = await service.AskFormattedText(
            template: expandPrompt,
            content: rawQuestion,
            model: model);

        var regex = new Regex(@"<questions>([\s\S]*?)</questions>", RegexOptions.IgnoreCase);
        var match = regex.Match(result);
        if(match.Success)
        {
            var questionsContent = match.Groups[1].Value.Trim();
            return questionsContent;
        }
        else
        {
            throw new Exception($"Failed to extract questions from the response: {result}.");
        }
    }

    private async Task<string> TrimQuestion(string rawQuestion, IUnderlyingService service, string model)
    {
        const string expandPrompt2 =
            """
            每天我们的客户会向我们询问大量的问题。为了解答这些问题，我们有一整套完整的流水线。

            其中，你扮演的角色是问题的收集人员。有些客户并不能特别清晰的表述他的问题。所以你的职责就是确定用户究竟想调查的是什么。

            为了调查一个问题，我需要你将这个问题进行标准化，变成一个易于后续流程调查的问题。你也可以将这个问题先进行格式化，修正其中的错别字，删除和最终调查无意义的信息。

            我并不需要你解决用户的问题。我需要你做的事情非常简单：将客户的问题格式化、凝练、修正，但不要丢失上下文信息。这样我们未来分别质询，可以方便后面的调查人员分组去进行调查。

            请确保你概括的问题内容中**包含必要的上下文信息**以方便后续调查人员调查。不要疏漏重要的信息。例如:

            * 如果原文提到了一个复杂的故事，那么你必须将故事同时概括一遍。
            * 你概括的内容中如果提到了原问题的信息，则务必同时概括引用的信息，否则后续的调查将很难进行。

            假如用户问了一大堆关于如何减脂的问题，那么你需要思考，然后输出格式形如：

            <questions>
            * 如何成功策划健身以减脂？
            * 如何确保一个减脂计划切实可行？
            * 如何验证减脂的效果？
            * 实施未来的减脂计划的过程中，可能遇到哪些问题？如何解决？
            * 我们的减脂计划是否是长周期的？如何确保它长周期可靠？
            * 在减脂过程中，是否需要注意什么别的，比如:
              * 如何在减脂期间作息以帮助减脂？
              * 在减脂期间应该如何饮食？
              * 穿着什么服装进行健身更好？
            * ... (上面只是例子。你可以添加、补充更多你可以补充的。至少1600字。不要在输出里包含这行内容） ...
            </questions>

            假如用户的问题问了一大堆如何让 Lunarlake CPU 在 Linux 下能够正确安装声卡驱动等，那么你需要思考，然后输出格式形如：

            <questions>
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
            * ... (上面只是例子。你可以添加、补充更多你可以补充的。至少1600字。不要在输出里包含这行内容） ...
            </questions>

            我只是给了你两个例子。现在，你可以开展你的工作了。你需要你自己标准化的格式化问题，得到我们真正开始调研的起点问题。（你不需要回答问题）我们会把你输出的内容交给其他调查人员去调查。

            最终真实用户提交上来的问题是：

            ======================
            {0}
            ======================

            请按照 <questions></questions> 的格式输出吧。
            """;
        const string expandPrompt =
            """
            我遇到了下面的问题：

            ======================
            {0}
            ======================

            但是这个问题太冗长了，我需要你修剪它一下：在确保上下文不丢失的情况下，有条件的去掉一些无意义的信息，格式化文中的错别字，使用标准的markdown语法来输出。

            你输出的格式化后的文本，必须将内容放置在 <questions></questions> 之间。请确保你概括的问题内容中**包含必要的上下文信息**以方便后续调查人员调查。不要疏漏重要的信息。

            现在，请按照 <questions></questions> 的格式输出吧。
            """;
        var result = await service.AskFormattedText(
            template: expandPrompt,
            content: rawQuestion,
            model: model);

        var regex = new Regex(@"<questions>([\s\S]*?)</questions>", RegexOptions.IgnoreCase);
        var match = regex.Match(result);
        if(match.Success)
        {
            var questionsContent = match.Groups[1].Value.Trim();
            return questionsContent;
        }
        else
        {
            throw new Exception($"Failed to trim questions from the response: {result}.");
        }
    }
}
