using Aiursoft.GptClient.Abstractions;

namespace Aiursoft.GptGateway.Services.Underlying;

public interface IUnderlyingService
{
    public string Name { get; }
    public Task<HttpResponseMessage> AskStream(OpenAiModel model);
    public Task<CompletionData> AskModel(OpenAiModel model);
}

public static class UnderlyingServiceExtensions
{
    public static Task<string> AskFormattedText(this IUnderlyingService service, string template, string content,
        string model)
    {
        var question = string.Format(template, content);
        return service.AskText(question, model);
    }

    public static async Task<string> AskText(this IUnderlyingService service, string question, string model)
    {
        var request = new OpenAiModel
        {
            Model = model,
            Messages =
            [
                new MessagesItem()
                {
                    Role = "user",
                    Content = question
                }
            ]
        };
        var response = await service.AskModel(request);
        if (response.Choices is { Count: > 0 })
        {
            return response.GetAnswerPart();
        }

        throw new Exception("No response from OpenAI");
    }
}
