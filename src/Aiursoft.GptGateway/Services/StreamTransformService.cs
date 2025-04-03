using System.Text;
using System.Text.Json;

namespace Aiursoft.GptGateway.Services;

public class StreamTransformService
{
    private readonly ILogger<StreamTransformService> _logger;

    public StreamTransformService(ILogger<StreamTransformService> logger)
    {
        _logger = logger;
    }

    public async Task CopyProxyHttpResponse(HttpContext context, HttpResponseMessage responseMessage, string sourceFormat, string targetModel)
    {
        if (responseMessage == null)
        {
            throw new ArgumentNullException(nameof(responseMessage));
        }

        var response = context.Response;
        response.StatusCode = (int)responseMessage.StatusCode;

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

        using var responseStream = await responseMessage.Content.ReadAsStreamAsync();

        if (sourceFormat == "OpenAI" && !string.IsNullOrEmpty(targetModel))
        {
            // Transform OpenAI format to Ollama format
            await TransformOpenAIToOllamaStream(responseStream, response.Body, targetModel, context.RequestAborted);
        }
        else
        {
            // Direct copy for Ollama or non-streaming responses
            var streamCopyBufferSize = 81920;
            await responseStream.CopyToAsync(response.Body, streamCopyBufferSize, context.RequestAborted);
        }
    }

    private async Task TransformOpenAIToOllamaStream(Stream input, Stream output, string targetModel, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(input, Encoding.UTF8);

        var isFirstChunk = true;
        var finishReason = string.Empty;

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Remove "data: " prefix if it exists (SSE format)
            if (line.StartsWith("data: "))
            {
                line = line.Substring(6);
            }

            // Skip "[DONE]" message
            if (line == "[DONE]") continue;

            try
            {
                // Parse OpenAI chunk
                var openAiChunk = JsonSerializer.Deserialize<OpenAIChunkResponse>(line);
                if (openAiChunk == null) continue;

                var choice = openAiChunk.Choices.FirstOrDefault();
                if (choice == null) continue;

                // First chunk contains role information
                if (isFirstChunk && choice.Delta.Role == "assistant")
                {
                    // For Ollama compatibility, add thinking tag at the start
                    await WriteOllamaChunk(output, targetModel, "<think>", false, cancellationToken);
                    await WriteOllamaChunk(output, targetModel, "\n\n", false, cancellationToken);
                    await WriteOllamaChunk(output, targetModel, "</think>", false, cancellationToken);
                    await WriteOllamaChunk(output, targetModel, "\n\n", false, cancellationToken);
                    isFirstChunk = false;
                    continue;
                }

                // Check for content
                var content = choice.Delta.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    await WriteOllamaChunk(output, targetModel, content, false, cancellationToken);
                }

                // Check for finish reason
                finishReason = choice.FinishReason;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse OpenAI chunk: {Line}", line);
            }
        }

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
public class OpenAIChunkResponse
{
    public required string Id { get; set; }
    public required string Object { get; set; }
    public required long Created { get; set; }
    public required string Model { get; set; }

    // ReSharper disable once CollectionNeverUpdated.Global
    public required List<OpenAIChoice> Choices { get; init; } = new();
}

public class OpenAIChoice
{
    public required OpenAIDelta Delta { get; set; }
    public required int Index { get; set; }
    public required string FinishReason { get; set; }
}

public class OpenAIDelta
{
    public required string Role { get; set; }
    public required string Content { get; set; }
}
