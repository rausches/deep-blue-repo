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

    [HttpGet("analyze")]
    public async Task<IActionResult> AnalyzeUx([FromQuery] string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return BadRequest(new { error = "URL is required." });
        }

        // Call the OpenAI service to analyze UX (now returns Dictionary<string, string>)
        Dictionary<string, string> result = await _OpenAiService.AnalyzeUx(url);

        // Return JSON response
        return Ok(result);  // Converts Dictionary<string, string> to JSON
    }
}
