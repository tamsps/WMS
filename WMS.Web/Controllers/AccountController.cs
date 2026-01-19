using Microsoft.AspNetCore.Mvc;
using WMS.Web.Models;
using WMS.Web.Services;

namespace WMS.Web.Controllers;

public class AccountController : Controller
{
    private readonly IApiService _apiService;

    public AccountController(IApiService apiService)
    {
        _apiService = apiService;
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

        var result = await _apiService.PostAsync<ApiResponse<AuthResponse>>("api/auth/login", loginDto);

        if (result?.IsSuccess == true && result.Data != null)
        {
            _apiService.SetAccessToken(result.Data.AccessToken);
            _apiService.SetRefreshToken(result.Data.RefreshToken);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

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
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

