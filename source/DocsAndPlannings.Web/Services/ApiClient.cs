using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DocsAndPlannings.Web.Services;

/// <summary>
/// HTTP client for communicating with the DocsAndPlannings REST API.
/// Handles authentication, error handling, and response deserialization.
/// </summary>
public sealed class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ApiClient> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        try
        {
            AddAuthenticationHeader();

            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GET request failed for endpoint: {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PostAsync<T>(string endpoint, object data)
    {
        try
        {
            AddAuthenticationHeader();

            string jsonData = JsonSerializer.Serialize(data, _jsonOptions);
            StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "POST request failed for endpoint: {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<T?> PutAsync<T>(string endpoint, object data)
    {
        try
        {
            AddAuthenticationHeader();

            string jsonData = JsonSerializer.Serialize(data, _jsonOptions);
            StringContent content = new(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PutAsync(endpoint, content);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return default;
            }

            response.EnsureSuccessStatusCode();

            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "PUT request failed for endpoint: {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string endpoint)
    {
        try
        {
            AddAuthenticationHeader();

            HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "DELETE request failed for endpoint: {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<byte[]> GetFileAsync(string endpoint)
    {
        try
        {
            AddAuthenticationHeader();

            HttpResponseMessage response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GET file request failed for endpoint: {Endpoint}", endpoint);
            throw;
        }
    }

    public async Task<bool> UploadFileAsync(string endpoint, IFormFile file)
    {
        try
        {
            AddAuthenticationHeader();

            using MultipartFormDataContent content = new();
            using Stream fileStream = file.OpenReadStream();
            using StreamContent streamContent = new(fileStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "File upload failed for endpoint: {Endpoint}, File: {FileName}",
                endpoint, file.FileName);
            throw;
        }
    }

    /// <summary>
    /// Adds the JWT authentication token from the cookie to the request header.
    /// </summary>
    private void AddAuthenticationHeader()
    {
        HttpContext? context = _httpContextAccessor.HttpContext;

        if (context is null)
        {
            return;
        }

        // Try to get JWT token from cookie
        string? token = context.Request.Cookies["AuthToken"];

        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
