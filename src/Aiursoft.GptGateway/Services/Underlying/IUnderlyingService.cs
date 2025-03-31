using Aiursoft.GptClient.Abstractions;

namespace Aiursoft.GptGateway.Services.Underlying;

public interface IUnderlyingService
{
    public string Name { get; }
    public Task<HttpResponseMessage> AskStream(OpenAiModel model);
    public Task<CompletionData> AskModel(OpenAiModel model);
}