using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/chat")] 
public class OpenAiApiController : Controller
{
    private readonly IOpenAiService _OpenAiService; 
    private readonly ILogger<OpenAiApiController> _logger; 

    public OpenAiApiController(IOpenAiService OpenAIService, ILogger<OpenAiApiController> logger)
    {
        _logger = logger;
        _OpenAiService = OpenAIService;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        string result = await _OpenAiService.GetChatResponse("Hello, how are you?");
        return Content(result, "application/json");
    }
}