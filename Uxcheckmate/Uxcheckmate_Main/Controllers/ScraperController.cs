using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Uxcheckmate_Main.Services;

namespace Scraper.Controllers
{
    [ApiController]
    [Route("scraper")]
    public class ScraperController : ControllerBase
    {
        private readonly WebScraperService _scraperService;

        public ScraperController(WebScraperService scraperService)
        {
            _scraperService = scraperService;
        }

        [HttpGet("extract")]
        public async Task<IActionResult> Extract([FromQuery] string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest(new { error = "URL parameter is required." });
            }

            try
            {
                var jsonData = await _scraperService.ScrapeAsync(url);
                return Ok(jsonData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
