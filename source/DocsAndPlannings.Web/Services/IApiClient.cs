namespace DocsAndPlannings.Web.Services;

/// <summary>
/// Client interface for communicating with the DocsAndPlannings REST API.
/// </summary>
public interface IApiClient
{
    /// <summary>
    /// Sends a GET request and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="endpoint">The API endpoint (relative path).</param>
    /// <returns>The deserialized response, or null if not found (404).</returns>
    Task<T?> GetAsync<T>(string endpoint);

    /// <summary>
    /// Sends a POST request with data and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="endpoint">The API endpoint (relative path).</param>
    /// <param name="data">The data to send in the request body.</param>
    /// <returns>The deserialized response, or null if not found (404).</returns>
    Task<T?> PostAsync<T>(string endpoint, object data);

    /// <summary>
    /// Sends a PUT request with data and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response to.</typeparam>
    /// <param name="endpoint">The API endpoint (relative path).</param>
    /// <param name="data">The data to send in the request body.</param>
    /// <returns>The deserialized response, or null if not found (404).</returns>
    Task<T?> PutAsync<T>(string endpoint, object data);

    /// <summary>
    /// Sends a DELETE request.
    /// </summary>
    /// <param name="endpoint">The API endpoint (relative path).</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> DeleteAsync(string endpoint);

    /// <summary>
    /// Downloads a file as byte array.
    /// </summary>
    /// <param name="endpoint">The API endpoint (relative path).</param>
    /// <returns>The file content as byte array.</returns>
    Task<byte[]> GetFileAsync(string endpoint);

    /// <summary>
    /// Uploads a file to the API.
    /// </summary>
    /// <param name="endpoint">The API endpoint (relative path).</param>
    /// <param name="file">The file to upload.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> UploadFileAsync(string endpoint, IFormFile file);
}
