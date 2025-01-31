using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Models.OpenAi;
using Aiursoft.GptGateway.Api.Services;
using Aiursoft.GptGateway.Api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.GptGateway.Api.Controllers;

public class HomeController(
    IEnumerable<IPreRequestMiddleware> preRequestMiddlewares,
    IEnumerable<IPostRequestMiddleware> postRequestMiddlewares,
    OpenAiService openAiService)
    : ControllerBase
{
    public async Task<IActionResult> Index()
    {
        var answer = await openAiService.AskOne("What is the meaning of life?", GptModel.DeepseekR132B);
        return Ok(answer);
    }
    
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
