using Aiursoft.GptGateway.Api.Models;
using Aiursoft.GptGateway.Api.Services;
using Aiursoft.GptGateway.Api.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Aiursoft.GptGateway.Api.Controllers;

public class HomeController : ControllerBase
{
    private readonly IEnumerable<IPreRequestMiddleware> _preRequestMiddlewares;
    private readonly IEnumerable<IPostRequestMiddleware> _postRequestMiddlewares;
    private readonly OpenAiService _openAiService;

    public HomeController(
        IEnumerable<IPreRequestMiddleware> preRequestMiddlewares,
        IEnumerable<IPostRequestMiddleware> postRequestMiddlewares,
        OpenAiService openAiService)
    {
        _preRequestMiddlewares = preRequestMiddlewares;
        _postRequestMiddlewares = postRequestMiddlewares;
        _openAiService = openAiService;
    }
    
    public async Task<IActionResult> Index()
    {
        var answer = await _openAiService.AskOne("What is the meaning of life?", GptModel.Gpt35Turbo);
        return Ok(answer);
    }
    
    [HttpPost]
    [Route("/v1/chat/completions")]
    public async Task<IActionResult> Ask([FromBody] OpenAiModel model)
    {
        var requestTime = DateTime.UtcNow;
        foreach (var middleware in _preRequestMiddlewares)
        {
            model = await middleware.PreRequest(HttpContext, model);
        }
        var answer = await _openAiService.AskModel(model, GptModel.Gpt432K);
        foreach (var middleware in _postRequestMiddlewares)
        {
            answer = await middleware.PostRequest(HttpContext, model, answer, requestTime);
        }
        return Ok(answer);
    }
}
