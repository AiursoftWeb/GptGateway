using Aiursoft.GptClient.Abstractions;

namespace Aiursoft.GptGateway.Services.Underlying;

public interface IUnderlyingService
{
    public string Name { get; }
    public Task<HttpResponseMessage> AskStream(OpenAiRequestModel model, CancellationToken cancellationToken);
    public Task<CompletionData> AskModel(OpenAiRequestModel model, CancellationToken cancellationToken);
    public bool SupportOllamaTooling { get; }
}

public static class UnderlyingServiceExtensions
{
    public static Task<string> AskFormattedText(this IUnderlyingService service, string template, string content,
        string model, CancellationToken cancellationToken)
    {
        var question = string.Format(template, content);
        return service.AskText(question, model, cancellationToken);
    }

    public static async Task<string> AskText(this IUnderlyingService service, string question, string model, CancellationToken cancellationToken)
    {
        var request = new OpenAiRequestModel
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
        var response = await service.AskModel(request, cancellationToken);
        if (response.Choices is { Count: > 0 })
        {
            return response.GetAnswerPart();
        }

        throw new Exception("No response from OpenAI");
    }
}
