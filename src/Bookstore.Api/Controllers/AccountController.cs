using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Api.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("[controller]")]
public sealed class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IAntiforgery _antiforgery;

    public AccountController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IAntiforgery antiforgery)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _antiforgery = antiforgery;
    }

    [HttpGet("Login")]
    public IActionResult Login(string? returnUrl)
    {
        return LoginPage(returnUrl);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login(
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] string? returnUrl)
    {
        if (!await HasValidAntiforgeryTokenAsync())
        {
            return BadRequest();
        }

        var result = await _signInManager.PasswordSignInAsync(
            username.Trim(),
            password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return LoginPage(returnUrl, result.IsLockedOut
                ? "This account is temporarily locked."
                : "Invalid username or password.");
        }

        return RedirectAfterSignIn(returnUrl);
    }

    [HttpGet("Register")]
    public IActionResult Register(string? returnUrl)
    {
        return RegisterPage(returnUrl);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register(
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] string? returnUrl)
    {
        if (!await HasValidAntiforgeryTokenAsync())
        {
            return BadRequest();
        }

        username = username.Trim();
        if (username.Length is < 3 or > 100)
        {
            return RegisterPage(returnUrl, "Username must contain between 3 and 100 characters.");
        }

        var user = new IdentityUser { UserName = username };
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            return RegisterPage(returnUrl, string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        return RedirectToAction(nameof(Login), new
        {
            returnUrl = string.IsNullOrWhiteSpace(returnUrl) || returnUrl == "/"
                ? null
                : returnUrl
        });
    }

    private IActionResult LoginPage(string? returnUrl, string? error = null)
    {
        var safeReturnUrl = WebUtility.HtmlEncode(returnUrl ?? "/");
        var registerUrl = $"/Account/Register?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
        var token = WebUtility.HtmlEncode(_antiforgery.GetAndStoreTokens(HttpContext).RequestToken);
        var errorHtml = string.IsNullOrWhiteSpace(error)
            ? string.Empty
            : $"<p class=\"error\">{WebUtility.HtmlEncode(error)}</p>";

        return Content($$"""
            <!doctype html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <title>Bookstore Login</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 40px; max-width: 420px; }
                    label { display: block; margin-top: 16px; }
                    input { width: 100%; padding: 8px; margin-top: 4px; }
                    button { margin-top: 20px; padding: 10px 14px; }
                    .error { color: #b42318; }
                </style>
            </head>
            <body>
                <h1>Bookstore Login</h1>
                {{errorHtml}}
                <form method="post" action="/Account/Login">
                    <input type="hidden" name="returnUrl" value="{{safeReturnUrl}}">
                    <input type="hidden" name="__RequestVerificationToken" value="{{token}}">
                    <label>
                        Username
                        <input name="username" autocomplete="username" required>
                    </label>
                    <label>
                        Password
                        <input name="password" type="password" autocomplete="current-password" required>
                    </label>
                    <button type="submit">Sign in</button>
                </form>
                <p><a href="{{registerUrl}}">Create an account</a></p>
            </body>
            </html>
            """, "text/html");
    }

    private IActionResult RegisterPage(string? returnUrl, string? error = null)
    {
        var safeReturnUrl = WebUtility.HtmlEncode(returnUrl ?? "/");
        var loginUrl = $"/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
        var token = WebUtility.HtmlEncode(_antiforgery.GetAndStoreTokens(HttpContext).RequestToken);
        var errorHtml = string.IsNullOrWhiteSpace(error)
            ? string.Empty
            : $"<p class=\"error\">{WebUtility.HtmlEncode(error)}</p>";

        return Content($$"""
            <!doctype html>
            <html lang="en">
            <head>
                <meta charset="utf-8">
                <title>Create Bookstore Account</title>
                <style>
                    body { font-family: Arial, sans-serif; margin: 40px; max-width: 420px; }
                    label { display: block; margin-top: 16px; }
                    input { width: 100%; padding: 8px; margin-top: 4px; }
                    button { margin-top: 20px; padding: 10px 14px; }
                    .error { color: #b42318; }
                </style>
            </head>
            <body>
                <h1>Create an account</h1>
                {{errorHtml}}
                <form method="post" action="/Account/Register">
                    <input type="hidden" name="returnUrl" value="{{safeReturnUrl}}">
                    <input type="hidden" name="__RequestVerificationToken" value="{{token}}">
                    <label>
                        Username
                        <input name="username" minlength="3" maxlength="100" autocomplete="username" required>
                    </label>
                    <label>
                        Password
                        <input name="password" type="password" minlength="8" autocomplete="new-password" required>
                    </label>
                    <button type="submit">Register</button>
                </form>
                <p><a href="{{loginUrl}}">Back to login</a></p>
            </body>
            </html>
            """, "text/html");
    }

    private IActionResult RedirectAfterSignIn(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || returnUrl == "/")
        {
            return Redirect("/swagger");
        }

        return Url.IsLocalUrl(returnUrl)
            ? LocalRedirect(returnUrl)
            : Redirect("/swagger");
    }

    private async Task<bool> HasValidAntiforgeryTokenAsync()
    {
        try
        {
            await _antiforgery.ValidateRequestAsync(HttpContext);
            return true;
        }
        catch (AntiforgeryValidationException)
        {
            return false;
        }
    }
}
