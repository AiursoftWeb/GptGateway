using Aiursoft.GptGateway.Models;
using Aiursoft.GptGateway.Models.OpenAi;
using Aiursoft.GptGateway.Services;
using Aiursoft.GptGateway.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.GptGateway.Controllers;

public class HomeController(
    IEnumerable<IPreRequestMiddleware> preRequestMiddlewares,
    IEnumerable<IPostRequestMiddleware> postRequestMiddlewares,
    OpenAiService openAiService)
    : ControllerBase
{
    [HttpPost]
    [Route("/v1/chat/completions")]
    public async Task<IActionResult> Ask([FromBody] OpenAiModel rawInput)
    {
        var context = new ConversationContext
        {
            HttpContext = HttpContext,
            RawInput = rawInput,
            ModifiedInput = rawInput.Clone(),
            Output = null
        };
        foreach (var middleware in preRequestMiddlewares)
        {
            await middleware.PreRequest(context);
        }
        context.Output = await openAiService.AskModel(context.ModifiedInput, GptModel.DeepseekR132B);
        foreach (var middleware in postRequestMiddlewares)
        {
            await middleware.PostRequest(context);
        }
        return Ok(context.Output);
    }
}
