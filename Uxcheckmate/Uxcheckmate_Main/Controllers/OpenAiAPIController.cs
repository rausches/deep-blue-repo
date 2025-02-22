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

   /* [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest request)
    {
        var issues = await _OpenAiService.AnalyzeWithOpenAI(request.Url);
        return Ok(issues);
    }*/
}

public class AnalyzeRequest
{
    public string Url { get; set; }
}   

