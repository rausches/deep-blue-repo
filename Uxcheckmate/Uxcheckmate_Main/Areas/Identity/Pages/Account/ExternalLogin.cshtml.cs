using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;

public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public ExternalLoginModel(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public IActionResult OnPost(string provider, string returnUrl = null)
    {
        // Ensure returnUrl has a default value
        returnUrl ??= Url.Content("~/");

        // This is where the external provider will send users back after auth
        var redirectUrl = Url.Page("./ExternalLoginCallback", values: new { returnUrl });

        // This sets up the provider (GitHub) redirect and state
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

        return new ChallengeResult(provider, properties);
    }
}
