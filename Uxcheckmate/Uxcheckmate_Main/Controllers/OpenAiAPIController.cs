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

    public class ImproveRequest
    {
        public string Message { get; set; }
        public string Category { get; set; }
    }

    [HttpPost("improve")]
    public async Task<IActionResult> Improve([FromBody] ImproveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message) || string.IsNullOrWhiteSpace(request.Category))
            return BadRequest("Invalid input");

        var improved = await _OpenAiService.ImproveMessageAsync(request.Message, request.Category);
        return Ok(improved);
    }
}

public class AnalyzeRequest
{
    public string Url { get; set; }
}   