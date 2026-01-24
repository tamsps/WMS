using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace WMS.Web.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private const string AccessTokenKey = "AccessToken";
    private const string RefreshTokenKey = "RefreshToken";

    public ApiService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;

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

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            SetAuthorizationHeader();
            var response = await _httpClient.GetAsync(endpoint);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(content);
            }

            return default;
        }
        catch
        {
            return default;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object? data = null)
    {
        try
        {
            SetAuthorizationHeader();
            var json = data != null ? JsonConvert.SerializeObject(data) : string.Empty;
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }

            return default;
        }
        catch
        {
            return default;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            SetAuthorizationHeader();
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }

            return default;
        }
        catch
        {
            return default;
        }
    }

    public async Task<T?> PatchAsync<T>(string endpoint, object? data = null)
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
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }

            return default;
        }
        catch
        {
            return default;
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
        return _httpContextAccessor.HttpContext?.Session.GetString(AccessTokenKey);
    }

    public void SetAccessToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _httpContextAccessor.HttpContext?.Session.Remove(AccessTokenKey);
        }
        else
        {
            _httpContextAccessor.HttpContext?.Session.SetString(AccessTokenKey, token);
        }
    }

    public void SetRefreshToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _httpContextAccessor.HttpContext?.Session.Remove(RefreshTokenKey);
        }
        else
        {
            _httpContextAccessor.HttpContext?.Session.SetString(RefreshTokenKey, token);
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
