namespace Aiursoft.GptGateway.Models.OpenAi;

/// <summary>
/// https://platform.openai.com/docs/models/gpt-3-5
/// </summary>
public enum GptModel
{
    /// <summary>
    ///  gpt-3.5-turbo	Currently points to gpt-3.5-turbo-0613.	4,096 tokens	Up to Sep 2021
    /// </summary>
    Gpt35Turbo,

    /// <summary>
    /// gpt-3.5-turbo-16k	Currently points to gpt-3.5-turbo-16k-0613.	16,385 tokens	Up to Sep 2021
    /// </summary>
    Gpt35Turbo16K,
    
    /// <summary>
    /// gpt-4	Currently points to gpt-4-0613. See continuous model upgrades.	8,192 tokens	Up to Sep 2021
    /// </summary>
    Gpt4,
    
    /// <summary>
    /// gpt-4-32k	Currently points to gpt-4-32k-0613. See continuous model upgrades. This model was never rolled out widely in favor of GPT-4 Turbo.	32,768 tokens	Up to Sep 2021
    /// </summary>
    Gpt432K,
    
    /// <summary>
    /// deepseek-r1	model.
    /// </summary>
    DeepseekR132B
}
