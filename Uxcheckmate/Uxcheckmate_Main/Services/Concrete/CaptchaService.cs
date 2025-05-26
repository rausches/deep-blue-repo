using System.Text.Json;
public class CaptchaService : ICaptchaService
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;
    public CaptchaService(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }
    public async Task<bool> VerifyTokenAsync(string token)
    {
        var secret = _config["Captcha:SecretKey"];
        var response = await _httpClientFactory.CreateClient().PostAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}", null);
        var result = JsonSerializer.Deserialize<Dictionary<string, object>>(await response.Content.ReadAsStringAsync());
        return result != null && result.TryGetValue("success", out var success) && success.ToString() == "True";
    }
} 


