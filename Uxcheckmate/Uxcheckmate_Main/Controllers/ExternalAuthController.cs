using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

public class ExternalAuthController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public ExternalAuthController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // Step 1: Kick off GitHub login
    [HttpGet("auth/github")]
    public IActionResult RedirectToProvider(string returnUrl = "/")
    {
        var redirectUrl = Url.Action("GitHubCallback", "ExternalAuth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("GitHub", redirectUrl);
        return Challenge(properties, "GitHub");
    }

    // Step 2: Handle GitHub callback
    [HttpGet("auth/github/callback")]
    public async Task<IActionResult> GitHubCallback(string returnUrl = "/")
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return View("LoginFailed");
        }

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
        if (result.Succeeded)
        {
            return Redirect(returnUrl);
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email == null)
        {
            return View("LoginFailed");
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email };
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return View("LoginFailed");
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
            return View("LoginFailed");

        await _signInManager.SignInAsync(user, false);
        return Redirect(returnUrl);
    }

    [HttpGet("auth/google")]
    public IActionResult RedirectToGoogle(string returnUrl = "/")
    {
        var redirectUrl = Url.Action("GoogleCallback", "ExternalAuth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return Challenge(properties, "Google");
    }

    [HttpGet("auth/google/callback")]
    public async Task<IActionResult> GoogleCallback(string returnUrl = "/")
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
            return View("LoginFailed");

        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
        if (result.Succeeded)
            return Redirect(returnUrl);

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (email == null)
            return View("LoginFailed");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email };
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                return View("LoginFailed");
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
            return View("LoginFailed");

        await _signInManager.SignInAsync(user, false);
        return Redirect(returnUrl);
    }
}
