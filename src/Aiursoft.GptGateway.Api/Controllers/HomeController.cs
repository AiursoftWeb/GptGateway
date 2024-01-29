using Microsoft.AspNetCore.Mvc;
using GptGateway.Services;

namespace Aiursoft.GptGateway.Controllers;

public class HomeController : ControllerBase
{
    private readonly OpenAiService _openAiService;

    public HomeController(OpenAiService openAiService)
    {
        _openAiService = openAiService;
    }
    
    public async Task<IActionResult> Index()
    {
        var answer = await _openAiService.Ask("What is the meaning of life?");
        return Ok(answer);
    }
    
    [HttpPost]
    [Route("/v1/chat/completions")]
    public async Task<IActionResult> Ask([FromBody] OpenAiModel model)
    {
        var answer = await _openAiService.Ask(model);
        return Ok(answer);
    }
}
