using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Application.DTOs.Auth;
using WMS.Application.Interfaces;
using System.Security.Claims;

namespace WMS.Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// User login - returns JWT access token and refresh token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Login attempt for user: {Username}", dto.Username);
        var result = await _authService.LoginAsync(dto);
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Login failed for user: {Username}", dto.Username);
            return Unauthorized(result);
        }
        
        _logger.LogInformation("Login successful for user: {Username}", dto.Username);
        return Ok(result);
    }

    /// <summary>
    /// User registration - creates new user account
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Registration attempt for user: {Username}", dto.Username);
        var result = await _authService.RegisterAsync(dto);
        
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Registration failed for user: {Username}", dto.Username);
            return BadRequest(result);
        }
        
        _logger.LogInformation("Registration successful for user: {Username}", dto.Username);
        return CreatedAtAction(nameof(GetProfile), null, result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RefreshTokenAsync(dto);
        
        if (!result.IsSuccess)
        {
            return Unauthorized(result);
        }
        
        return Ok(result);
    }

    /// <summary>
    /// User logout - invalidates refresh token
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest(new { IsSuccess = false, Errors = new[] { "Invalid token" } });
        }

        var result = await _authService.LogoutAsync(username);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        
        _logger.LogInformation("User logged out: {Username}", username);
        return Ok(result);
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        var profile = new
        {
            UserId = userId,
            Username = username,
            Email = email,
            Roles = roles
        };

        return Ok(new { IsSuccess = true, Data = profile });
    }

    /// <summary>
    /// Validate token (check if current token is still valid)
    /// </summary>
    [HttpGet("validate")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        // If we reach here, the token is valid (passed [Authorize] filter)
        return Ok(new 
        { 
            IsSuccess = true, 
            Data = new 
            { 
                IsValid = true,
                Username = User.FindFirst(ClaimTypes.Name)?.Value,
                ExpiresAt = User.FindFirst("exp")?.Value
            }
        });
    }

    /// <summary>
    /// Get authentication statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetStatistics()
    {
        // In a real implementation, you would query the database for these stats
        var stats = new
        {
            TotalUsers = 0,
            ActiveSessions = 0,
            TodayLogins = 0,
            FailedLoginAttempts = 0
        };

        return Ok(new { IsSuccess = true, Data = stats });
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new 
        { 
            Service = "WMS.Auth.API",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}
