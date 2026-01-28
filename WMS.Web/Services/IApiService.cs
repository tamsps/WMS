using WMS.Web.Models;

namespace WMS.Web.Services;

public interface IApiService
{
    Task<ApiResponse<T>> GetAsync<T>(string endpoint);
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null);
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data);
    Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null);
    Task<bool> DeleteAsync(string endpoint);
    string? GetAccessToken();
    void SetAccessToken(string? token);
    void SetRefreshToken(string? token);
    string? GetRefreshToken();
    void ClearTokens();
}
