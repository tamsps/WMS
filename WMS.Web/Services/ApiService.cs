using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using WMS.Web.Models;

namespace WMS.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiService> _logger;
    private const string AccessTokenKey = "AccessToken";
    private const string RefreshTokenKey = "RefreshToken";

    public ApiService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<ApiService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;

        var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5000";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    private void SetAuthorizationHeader()
    {
        var token = GetAccessToken();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(content);
            if (apiResponse == null)
            {
                apiResponse = new ApiResponse<T> { IsSuccess = false, Errors = new List<string> { "Failed to deserialize response" } };
            }
            
            return apiResponse;
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { IsSuccess = false, Errors = new List<string> { ex.Message } };
        }
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            SetAuthorizationHeader();
            var json = data != null ? JsonConvert.SerializeObject(data) : string.Empty;
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
            if (apiResponse == null)
            {
                apiResponse = new ApiResponse<T> { IsSuccess = false, Errors = new List<string> { "Failed to deserialize response" } };
            }
            
            return apiResponse;
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { IsSuccess = false, Errors = new List<string> { ex.Message } };
        }
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            SetAuthorizationHeader();
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
            if (apiResponse == null)
            {
                apiResponse = new ApiResponse<T> { IsSuccess = false, Errors = new List<string> { "Failed to deserialize response" } };
            }
            
            return apiResponse;
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { IsSuccess = false, Errors = new List<string> { ex.Message } };
        }
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            SetAuthorizationHeader();
            var json = data != null ? JsonConvert.SerializeObject(data) : string.Empty;
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<T>>(responseContent);
            if (apiResponse == null)
            {
                apiResponse = new ApiResponse<T> { IsSuccess = false, Errors = new List<string> { "Failed to deserialize response" } };
            }
            
            return apiResponse;
        }
        catch (Exception ex)
        {
            return new ApiResponse<T> { IsSuccess = false, Errors = new List<string> { ex.Message } };
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public string? GetAccessToken()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            _logger.LogWarning("HttpContext.Session is null when trying to get access token");
            return null;
        }

        var token = session.GetString(AccessTokenKey);
        _logger.LogInformation("GetAccessToken called. Token {Status}. SessionId: {SessionId}", 
            string.IsNullOrEmpty(token) ? "NOT FOUND" : "FOUND", 
            session.Id ?? "NO_SESSION_ID");
        
        return token;
    }

    public void SetAccessToken(string? token)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            _logger.LogError("HttpContext.Session is null when trying to set access token");
            return;
        }

        if (string.IsNullOrEmpty(token))
        {
            session.Remove(AccessTokenKey);
            _logger.LogInformation("Access token removed from session. SessionId: {SessionId}", session.Id);
        }
        else
        {
            session.SetString(AccessTokenKey, token);
            _logger.LogInformation("Access token set in session. Token length: {Length}, SessionId: {SessionId}", 
                token.Length, session.Id ?? "NO_SESSION_ID");
        }
    }

    public void SetRefreshToken(string? token)
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        if (session == null)
        {
            _logger.LogError("HttpContext.Session is null when trying to set refresh token");
            return;
        }

        if (string.IsNullOrEmpty(token))
        {
            session.Remove(RefreshTokenKey);
        }
        else
        {
            session.SetString(RefreshTokenKey, token);
            _logger.LogInformation("Refresh token set in session. Token length: {Length}", token.Length);
        }
    }

    public string? GetRefreshToken()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString(RefreshTokenKey);
    }

    public void ClearTokens()
    {
        _httpContextAccessor.HttpContext?.Session.Remove(AccessTokenKey);
        _httpContextAccessor.HttpContext?.Session.Remove(RefreshTokenKey);
    }
}
