namespace WMS.Web.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint);
    Task<T?> PostAsync<T>(string endpoint, object? data = null);
    Task<T?> PutAsync<T>(string endpoint, object data);
    Task<bool> DeleteAsync(string endpoint);
    Task<T?> PatchAsync<T>(string endpoint, object? data = null);
    string? GetAccessToken();
    void SetAccessToken(string? token);
    void SetRefreshToken(string? token);
    string? GetRefreshToken();
    void ClearTokens();
}
