using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;

public class ExternalLoginCallbackModel : PageModel
{/*
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public ExternalLoginCallbackModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<IActionResult> OnGetAsync(string returnUrl = null, string remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            TempData["ErrorMessage"] = $"Error from external provider: {remoteError}";
            return RedirectToPage("./Login");
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            TempData["ErrorMessage"] = "Error loading external login info.";
            return RedirectToPage("./Login");
        }

        // Try to sign in user with this external login provider
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        // Attempt to get email from GitHub
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "GitHub did not provide an email address. Please ensure your email is public or add it in your GitHub profile.";
            return RedirectToPage("./Login");
        }

        // Check if user with this email already exists
        var user = await _userManager.FindByEmailAsync(email);
        if (user != null)
        {
            TempData["ErrorMessage"] = "An account with this email already exists. Please log in with your email and password.";
            return RedirectToPage("./Login");
        }

        // Create new user and link GitHub login
        user = new IdentityUser { UserName = email, Email = email };
        var createResult = await _userManager.CreateAsync(user);
        if (createResult.Succeeded)
        {
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (addLoginResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
        }

        foreach (var error in createResult.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        TempData["ErrorMessage"] = "Could not complete sign in with GitHub.";
        return RedirectToPage("./Login");
    }
*/}
