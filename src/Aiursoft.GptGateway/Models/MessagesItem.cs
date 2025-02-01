using System.Text.Json.Serialization;

namespace Aiursoft.GptGateway.Models;

public class MessagesItem
{
    [JsonPropertyName("role")] public string? Role { get; set; }

    [JsonPropertyName("content")] public string? Content { get; set; }

    [JsonIgnore] public bool IsInjected { get; set; }
    
    public MessagesItem Clone()
    {
        return new MessagesItem
        {
            Role = Role,
            Content = Content,
            IsInjected = IsInjected
        };
    }
}