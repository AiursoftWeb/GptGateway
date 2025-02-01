using Aiursoft.GptGateway.Models;

namespace Aiursoft.GptGateway.Services;

public class QuestionReformatService(ILogger<QuestionReformatService> logger)
{
    public OpenAiModel Map(OpenAiModel model, string template, int take, bool includeSystemMessage, out string lastRawQuestion, bool mergeAsOne = false)
    {
        logger.LogInformation("Formatting Question to get plugin input: {0}", model.Messages.LastOrDefault()?.Content);
        var messagesQuery = model.Messages
            //.Where(m => !m.IsInjected);
            .AsEnumerable();
        lastRawQuestion = model.Messages.LastOrDefault()?.Content!;

        if (!includeSystemMessage)
        {
            messagesQuery = messagesQuery
                .Where(m => m.Role == "user");
        }
        messagesQuery = messagesQuery.TakeLast(take);
        var messages = messagesQuery.ToArray();
        
        if (mergeAsOne)
        {
            var mergedContent = string.Join("\n", messages.Select(m => m.Content));
            return new OpenAiModel
            {
                Messages = new List<MessagesItem>
                {
                    new()
                    {
                        Role = "user",
                        Content = string.Format(template, mergedContent),
                    }
                }
            };
        }
        else
        {
            var messagesArray = messages.ToArray();
            messagesArray[^1] = new MessagesItem
            {
                Role = "user",
                Content = string.Format(template, messagesArray[^1].Content),
            };
            return new OpenAiModel
            {
                Messages = messagesArray.ToList(),
            };
        }
    }
    
    public int ConvertResponseToScore(CompletionData response)
    {
        var responseLastOutput = response.GetContent();
        logger.LogInformation("Plugin output: {0}", responseLastOutput);
        
        var truePosition = responseLastOutput.IndexOf("true", StringComparison.Ordinal);
        var falsePosition = responseLastOutput.IndexOf("false", StringComparison.Ordinal);
        // Has true no false, return 70
        // Has false no true, return 0
        // Has true and false, return 40 if true is before false, otherwise return 0
        // Has no true and false, return 10
        if (truePosition >= 0 && falsePosition == -1)
        {
            return 70;
        }
        if (falsePosition >= 0 && truePosition == -1)
        {
            return 0;
        }
        if (truePosition >= 0 && falsePosition >= 0)
        {
            return truePosition < falsePosition ? 40 : 0;
        }
        return 10;
    }
}