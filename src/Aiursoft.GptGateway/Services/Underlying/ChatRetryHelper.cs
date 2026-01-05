using Aiursoft.GptClient.Abstractions;
using Aiursoft.GptClient.Services;

namespace Aiursoft.GptGateway.Services.Underlying;

public static class ChatRetryHelper
{
    public static async Task<CompletionData> AskModelWithRetry(
        this ChatClient client,
        OpenAiRequestModel model,
        string completionApiUrl,
        string? token,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        const int maxAttempts = 4;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await client.AskModel(model, completionApiUrl, token, cancellationToken);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Both Choices and Message are empty"))
            {
                if (attempt == maxAttempts)
                {
                    logger.LogError(ex, "Failed to ask model after {MaxAttempts} attempts. Returning empty response.", maxAttempts);
                    return new CompletionData
                    {
                        Choices = []
                    };
                }

                var delay = (int)Math.Min(Math.Pow(2, attempt) * 1000, 32000); 
                logger.LogWarning(ex, "Failed to ask model (attempt {Attempt}/{MaxAttempts}). Both Choices and Message are empty. Retrying in {Delay}ms...", attempt, maxAttempts, delay);
                await Task.Delay(delay, cancellationToken);
            }
        }
        
        return new CompletionData { Choices = [] };
    }
}
