using Aiursoft.GptGateway.Models;
using System.Text;
using System.Text.Json.Serialization;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Aiursoft.GptGateway.Services;

public class StreamTransformService(
    RequestLogContext logContext,
    ILogger<StreamTransformService> logger)
{
    public async Task CopyProxyHttpResponse(HttpContext context, HttpResponseMessage responseMessage, string sourceFormat, string targetModel, CancellationToken cancellationToken)
    {
        var response = context.Response;
        response.StatusCode = (int)responseMessage.StatusCode;
        logContext.Log.Success = responseMessage.IsSuccessStatusCode;

        // Copy all headers except content-length as we'll be modifying the content
        foreach (var header in responseMessage.Headers)
        {
            if (!string.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }
        }

        foreach (var header in responseMessage.Content.Headers)
        {
            if (!string.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase))
            {
                response.Headers[header.Key] = header.Value.ToArray();
            }
        }

        // Set content type to application/json for streaming
        response.Headers["Content-Type"] = "application/json";

        // Remove transfer-encoding as we'll handle the streaming ourselves
        response.Headers.Remove("transfer-encoding");

        await using var responseStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);

        if ((sourceFormat == "OpenAI" || sourceFormat == "DeepSeek") && !string.IsNullOrEmpty(targetModel))
        {
            // Transform OpenAI format to Ollama format
            await TransformOpenAiToOllamaStream(responseStream, response.Body, targetModel, cancellationToken);
        }
        else
        {
            // Direct copy for Ollama or non-streaming responses
            const int streamCopyBufferSize = 81920;
            // For direct copy, we might not be able to easily capture the answer unless we wrap the stream.
            // But since this is a proxy, we'll try to capture if it's feasible.
            // However, the requirement is to log answer. 
            // If it's direct copy, it's likely Ollama format.
            await responseStream.CopyToAsync(response.Body, streamCopyBufferSize, cancellationToken);
        }
    }

    private async Task TransformOpenAiToOllamaStream(Stream input, Stream output, string targetModel, CancellationToken cancellationToken)
    {
        // Use leaveOpen: true if you don't want the StreamReader to dispose the input stream
        using var reader = new StreamReader(input, Encoding.UTF8, leaveOpen: true);

        var finishReason = string.Empty;
        var answerBuilder = new StringBuilder();
        var thinkingBuilder = new StringBuilder();

        // --- FIX for CA2024 ---
        // We declare 'line' outside the loop.
        string? line;

        // The loop condition is changed from checking 'reader.EndOfStream' (which can block)
        // to awaiting ReadLineAsync() and checking its result for 'null'.
        // This is the correct asynchronous pattern.
        // The CancellationToken is passed to ReadLineAsync, which will throw
        // an OperationCanceledException if cancellation is requested during the await.
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            // The original 'var line = ...' is no longer needed here.

            if (string.IsNullOrWhiteSpace(line)) continue;

            // Remove "data: " prefix if it exists (SSE format)
            if (line.StartsWith("data: "))
            {
                line = line[6..];
            }

            // Skip "[DONE]" message
            if (line == "[DONE]") continue;

            try
            {
                // Parse OpenAI chunk
                var openAiChunk = JsonSerializer.Deserialize<OpenAiChunkResponse>(line);
                if (openAiChunk == null) continue;

                var choice = openAiChunk.Choices.FirstOrDefault();
                if (choice == null) continue;

                // Check for thinking content
                var thinking = choice.Delta.ReasoningContent;
                if (!string.IsNullOrEmpty(thinking))
                {
                    thinkingBuilder.Append(thinking);
                    // Ollama format usually doesn't have a separate field for thinking in the chunk itself,
                    // but we can just pass it through as content if we want, or handle it differently.
                    // For now, we capture it for logging.
                    await WriteOllamaChunk(output, targetModel, thinking, false, cancellationToken);
                }

                // Check for content
                var content = choice.Delta.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    answerBuilder.Append(content);
                    await WriteOllamaChunk(output, targetModel, content, false, cancellationToken);
                }

                // Check for finish reason
                if (choice.FinishReason != null)
                {
                    finishReason = choice.FinishReason;
                }
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to parse OpenAI chunk: {Line}", line);
            }
        }
        // --- End of FIX ---

        logContext.Log.Answer = answerBuilder.ToString();
        logContext.Log.Thinking = thinkingBuilder.ToString();

        // Write final chunk
        await WriteOllamaChunk(output, targetModel, "", true, cancellationToken, finishReason);
    }

    private async Task WriteOllamaChunk(Stream output, string model, string content, bool done, CancellationToken cancellationToken, string? doneReason = null)
    {
        var ollamaResponse = new
        {
            model,
            created_at = DateTime.UtcNow.ToString("o"),
            message = new
            {
                role = "assistant", content
            },
            done
        };

        // Add done_reason and statistics for the final chunk
        dynamic finalResponse = done ? new
            {
                model,
                created_at = DateTime.UtcNow.ToString("o"),
                message = new
                {
                    role = "assistant",
                    content
                },
                done_reason = string.IsNullOrEmpty(doneReason) ? "stop" : doneReason,
                done = true,
                total_duration = 1000000000, // Placeholder values
                load_duration = 10000000,
                prompt_eval_count = 5,
                prompt_eval_duration = 100000000,
                eval_count = 15,
                eval_duration = 900000000
            } : ollamaResponse;

        var json = JsonSerializer.Serialize(finalResponse);
        var bytes = Encoding.UTF8.GetBytes(json + "\n");

        await output.WriteAsync(bytes, cancellationToken);
        await output.FlushAsync(cancellationToken);
    }
}

// OpenAI response model classes
public class OpenAiChunkResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("object")]
    public required string Object { get; init; }

    [JsonPropertyName("created")]
    public required long Created { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    // ReSharper disable once CollectionNeverUpdated.Global
    [JsonPropertyName("choices")]
    public required List<OpenAiChoice> Choices { get; init; } = new();
}

// ReSharper disable once ClassNeverInstantiated.Global
public class OpenAiChoice
{
    [JsonPropertyName("delta")]
    public required OpenAiDelta Delta { get; init; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }
}

public class OpenAiDelta
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("reasoning_content")]
    public string? ReasoningContent { get; set; }
}
