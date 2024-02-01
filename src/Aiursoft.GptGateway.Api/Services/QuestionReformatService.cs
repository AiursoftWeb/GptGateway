using Aiursoft.GptGateway.Api.Models;

namespace Aiursoft.GptGateway.Api.Services;

public class QuestionReformatService
{
    private readonly ILogger<QuestionReformatService> _logger;

    public QuestionReformatService(ILogger<QuestionReformatService> logger)
    {
        _logger = logger;
    }
    
    public OpenAiModel Map(OpenAiModel model, string template, int take, bool includeSystemMessage, out string lastRawQuestion)
    {
        _logger.LogInformation("Formatting Question to get plugin input: {0}", model.Messages.LastOrDefault()?.Content);
        var messagesQuery = model.Messages.AsEnumerable();
        if (!includeSystemMessage)
        {
            messagesQuery = messagesQuery.Where(m => m.Role == "user");
        }
        messagesQuery = messagesQuery.TakeLast(take);
        var messages = messagesQuery.ToArray();
        
        lastRawQuestion = model.Messages.LastOrDefault()?.Content!;
        var formattedFinalQuestion = string.Format(template, lastRawQuestion);
        messages[^1] = new MessagesItem
        {
            Role = "user",
            Content = formattedFinalQuestion,
        };
        var requestModel = new OpenAiModel
        {
            Messages = messages.ToList(),
        };
        
        return requestModel;
    }
    
    public int ConvertResponseToScore(CompletionData response)
    {
        var responseLastOutput = response.Choices.FirstOrDefault()!.Message!.Content!;
        _logger.LogInformation("Plugin output: {0}", responseLastOutput);
        
        var truePosition = responseLastOutput.IndexOf("true", StringComparison.Ordinal);
        var falsePosition = responseLastOutput.IndexOf("false", StringComparison.Ordinal);
        // Has true no false, return 70
        // Has false no true, return 0
        // Has true and false, return 40 if true is before false, otherwise return 0
        // Has no true and false, return 10
        if (truePosition > 0 && falsePosition == -1)
        {
            return 70;
        }
        if (falsePosition > 0 && truePosition == -1)
        {
            return 0;
        }
        if (truePosition > 0 && falsePosition > 0)
        {
            return truePosition < falsePosition ? 40 : 0;
        }
        return 10;
    }
}