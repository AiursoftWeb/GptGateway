using System.ComponentModel.DataAnnotations;

namespace Aiursoft.GptGateway.Api.Models.Database;

public class UserConversation
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(64)]
    public required string RequestIpAddress { get; set; }
    
    [MaxLength(128)]
    public required string RequestUserAgent { get; set; }
    
    [MaxLength(65536)]
    public required string Questions { get; set; }
    
    [MaxLength(65536)]
    public required string Answer { get; set; }
    
    public required TimeSpan Duration { get; set; }
    
    public required DateTime ConversationTime { get; set; }
}