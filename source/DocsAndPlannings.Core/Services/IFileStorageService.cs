namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service interface for file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to storage
    /// </summary>
    /// <param name="fileStream">The file stream to save</param>
    /// <param name="fileName">The original file name</param>
    /// <param name="contentType">The MIME type of the file</param>
    /// <returns>The stored file name (with unique identifier)</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType);

    /// <summary>
    /// Retrieves a file from storage
    /// </summary>
    /// <param name="storedFileName">The stored file name</param>
    /// <returns>A tuple containing the file stream and content type</returns>
    Task<(Stream FileStream, string ContentType)> GetFileAsync(string storedFileName);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="storedFileName">The stored file name</param>
    Task DeleteFileAsync(string storedFileName);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    /// <param name="storedFileName">The stored file name</param>
    /// <returns>True if the file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(string storedFileName);

    /// <summary>
    /// Gets the full path to a stored file
    /// </summary>
    /// <param name="storedFileName">The stored file name</param>
    /// <returns>The full file path</returns>
    string GetFilePath(string storedFileName);

    /// <summary>
    /// Validates file type and size
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <param name="contentType">The MIME type</param>
    /// <param name="fileSizeBytes">The file size in bytes</param>
    /// <exception cref="BadRequestException">Thrown when file is invalid</exception>
    void ValidateFile(string fileName, string contentType, long fileSizeBytes);
}
