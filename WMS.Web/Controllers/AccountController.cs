using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

public class AccountController : Controller
{
    private readonly IApiService _apiService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IApiService apiService, ILogger<AccountController> logger)
    {
        _apiService = apiService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (!string.IsNullOrEmpty(_apiService.GetAccessToken()))
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var loginDto = new
        {
            Username = model.Username,
            Password = model.Password
        };

        _logger.LogInformation("Attempting login for user: {Username}", model.Username);

        var result = await _apiService.PostAsync<ApiResponse<AuthResponse>>("auth/login", loginDto);

        if (result?.IsSuccess == true && result.Data != null)
        {
            _logger.LogInformation("Login successful for user: {Username}", model.Username);
            
            var token = result.Data.Token;
            var refreshToken = result.Data.RefreshToken;
            
            _logger.LogInformation("Token received - Length: {TokenLength}, RefreshToken Length: {RefreshTokenLength}", 
                token?.Length ?? 0, refreshToken?.Length ?? 0);

            _apiService.SetAccessToken(token);
            _apiService.SetRefreshToken(refreshToken);

            // Ensure session is committed before redirect
            await HttpContext.Session.CommitAsync();
            
            _logger.LogInformation("Session committed. Verifying token storage...");
            
            var storedToken = _apiService.GetAccessToken();
            _logger.LogInformation("Token retrieved from session - Length: {StoredTokenLength}", 
                storedToken?.Length ?? 0);

            if (string.IsNullOrEmpty(storedToken))
            {
                _logger.LogError("CRITICAL: Token was not stored in session!");
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        _logger.LogWarning("Login failed for user: {Username}. Message: {Message}", 
            model.Username, result?.Message);
        ModelState.AddModelError(string.Empty, result?.Message ?? "Login failed");
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var registerDto = new
        {
            Username = model.Username,
            Email = model.Email,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        var result = await _apiService.PostAsync<ApiResponse<AuthResponse>>("api/auth/register", registerDto);

        if (result?.IsSuccess == true)
        {
            TempData["SuccessMessage"] = "Registration successful! Please login.";
            return RedirectToAction(nameof(Login));
        }

        ModelState.AddModelError(string.Empty, result?.Message ?? "Registration failed");
        return View(model);
    }

    [HttpPost]
    public IActionResult Logout()
    {
        _apiService.ClearTokens();
        return RedirectToAction(nameof(Login));
    }
}

// Helper classes for API responses
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string AccessToken => Token; // Compatibility alias
    public string RefreshToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

