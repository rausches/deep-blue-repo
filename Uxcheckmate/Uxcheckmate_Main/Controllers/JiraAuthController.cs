using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Uxcheckmate_Main.Controllers
{
    [Authorize]
    [Route("JiraAuth")]
    public class JiraAuthController : Controller
    {
        private readonly IConfiguration _config;

        public JiraAuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("Authorize")]
        public IActionResult Authorize(int reportId)
        {
            var clientId = _config["Jira:ClientId"];
            var redirectUri = _config["Jira:RedirectUri"];
            var scopes = "read%3Ajira-user%20read%3Ajira-work%20write%3Ajira-work";

            // ✅ Use OAuth "state" parameter to carry reportId safely
            var authorizationUrl = $"https://auth.atlassian.com/authorize?audience=api.atlassian.com&client_id={clientId}&scope={scopes}&redirect_uri={redirectUri}&response_type=code&prompt=consent&state={reportId}";

            return Redirect(authorizationUrl);
        }

        [HttpGet("Callback")]
        public async Task<IActionResult> Callback(string code, string state)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Authorization code missing.");

            var clientId = _config["Jira:ClientId"];
            var clientSecret = _config["Jira:ClientSecret"];
            var redirectUri = _config["Jira:RedirectUri"];

            // ✅ Grab reportId from state
            if (!int.TryParse(state, out int reportId))
                return BadRequest("Missing or invalid state (reportId).");

            using var httpClient = new HttpClient();
            var payload = new
            {
                grant_type = "authorization_code",
                client_id = clientId,
                client_secret = clientSecret,
                code = code,
                redirect_uri = redirectUri
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var tokenResponse = await httpClient.PostAsync("https://auth.atlassian.com/oauth/token", content);
            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<JiraOAuthResponse>();

            // ✅ Store tokens in session
            HttpContext.Session.SetString("JiraAccessToken", tokenResult.access_token);
            var cloudId = await GetJiraCloudId(tokenResult.access_token);
            HttpContext.Session.SetString("JiraCloudId", cloudId);

            // ✅ Redirect with resumeReportId to auto-run export
            return Redirect($"/Home/UserDash?resumeReportId={reportId}");
        }

        private async Task<string> GetJiraCloudId(string accessToken)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.GetFromJsonAsync<List<JiraCloudSite>>("https://api.atlassian.com/oauth/token/accessible-resources");
            return response.First().id;
        }

        public class JiraOAuthResponse
        {
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string token_type { get; set; }
            public string scope { get; set; }
        }

        public class JiraCloudSite
        {
            public string id { get; set; }
            public string url { get; set; }
            public string name { get; set; }
        }
    }
}
