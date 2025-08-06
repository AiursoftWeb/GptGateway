using Aiursoft.CSTools.Tools;
using Aiursoft.GptClient.Abstractions;

namespace Aiursoft.GptGateway.Services;

public class QuestionReformatService(ILogger<QuestionReformatService> logger)
{
    public string MergeAsString(OpenAiRequestModel model, int take, out string lastRawQuestion)
    {
        var mapped = Map(model, "{0}", take, out lastRawQuestion, true);
        return mapped.Messages.First().Content!;
    }

    public OpenAiRequestModel Map(OpenAiRequestModel model, string template, int take, out string lastRawQuestion, bool mergeAsOne = false)
    {
        logger.LogInformation("Formatting Question: {LastMessageContent}", model.Messages.LastOrDefault()?.Content!.SafeSubstring(80));
        var messagesQuery = model.Messages
            .AsEnumerable();
        lastRawQuestion = model.Messages.LastOrDefault()?.Content!;

        messagesQuery = messagesQuery.TakeLast(take);
        var messages = messagesQuery.ToArray();

        if (mergeAsOne)
        {
            var mergedContent = string.Join("\n", messages.Select(m => m.Content));
            model.Messages =
            [
                new MessagesItem
                {
                    Role = "user",
                    Content = string.Format(template, mergedContent),
                }
            ];
        }
        else
        {
            var messagesArray = messages.ToArray();
            messagesArray[^1] = new MessagesItem
            {
                Role = "user",
                Content = string.Format(template, messagesArray[^1].Content),
            };
            model.Messages = messagesArray.ToList();
        }

        return model;
    }
}
