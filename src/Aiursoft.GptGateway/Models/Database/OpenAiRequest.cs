using System.ComponentModel.DataAnnotations;

namespace Aiursoft.GptGateway.Models.Database;

public class OpenAiRequest
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(65536)]
    public required string LastQuestion { get; set; }
    
    [MaxLength(65536)]
    public required string Questions { get; set; }
    
    [MaxLength(65536)]
    public required string Answer { get; set; }
    
    public required TimeSpan Duration { get; set; }
    
    public required DateTime ConversationTime { get; set; }
    
    public int PromptTokens { get; set; }
    
    public int CompletionTokens { get; set; }
    
    public int TotalTokens { get; set; }
    
    public int PreTokenCount { get; set; }
    
    public int PreTotal { get; set; }
    
    public int AdjustTotal { get; set; }
    
    public int FinalTotal { get; set; }
}