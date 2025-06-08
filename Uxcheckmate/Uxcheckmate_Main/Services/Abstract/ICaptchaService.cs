public interface ICaptchaService
{
    Task<bool> VerifyTokenAsync(string token);
}