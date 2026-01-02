namespace Aiursoft.GptGateway.Models;

public class RequestLogContext
{
    public RequestLog Log { get; } = new()
    {
        RequestTime = DateTime.UtcNow
    };
}
