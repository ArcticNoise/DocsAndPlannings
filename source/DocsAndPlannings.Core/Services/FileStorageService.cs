using System.Diagnostics;
using DocsAndPlannings.Core.Exceptions;

namespace DocsAndPlannings.Core.Services;

/// <summary>
/// Service for file storage operations using the local filesystem
/// </summary>
public sealed class FileStorageService : IFileStorageService
{
    private const long MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024; // 10 MB
    private const string STORAGE_DIRECTORY = "screenshots";

    private static readonly HashSet<string> ALLOWED_CONTENT_TYPES = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp"
    };

    private static readonly HashSet<string> ALLOWED_EXTENSIONS = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp",
        ".bmp"
    };

    private readonly string _storagePath;

    /// <summary>
    /// Initializes a new instance of the FileStorageService class
    /// </summary>
    public FileStorageService()
    {
        // Storage path is relative to the application root
        _storagePath = Path.Combine(Directory.GetCurrentDirectory(), STORAGE_DIRECTORY);

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    /// <inheritdoc/>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
    {
        Debug.Assert(fileStream != null, "File stream cannot be null");
        Debug.Assert(!string.IsNullOrWhiteSpace(fileName), "File name cannot be empty");
        Debug.Assert(!string.IsNullOrWhiteSpace(contentType), "Content type cannot be empty");

        ValidateFile(fileName, contentType, fileStream.Length);

        // Generate unique file name
        string extension = Path.GetExtension(fileName);
        string storedFileName = $"{Guid.NewGuid()}{extension}";
        string filePath = GetFilePath(storedFileName);

        try
        {
            // Save file to disk
            using FileStream outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(outputStream);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to save file: {ex.Message}", ex);
        }

        return storedFileName;
    }

    /// <inheritdoc/>
    public async Task<(Stream FileStream, string ContentType)> GetFileAsync(string storedFileName)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(storedFileName), "Stored file name cannot be empty");

        string filePath = GetFilePath(storedFileName);

        if (!await FileExistsAsync(storedFileName))
        {
            throw new NotFoundException($"File '{storedFileName}' not found");
        }

        try
        {
            // Open file stream
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Determine content type from extension
            string extension = Path.GetExtension(storedFileName).ToLowerInvariant();
            string contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };

            return (fileStream, contentType);
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to read file: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public Task DeleteFileAsync(string storedFileName)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(storedFileName), "Stored file name cannot be empty");

        string filePath = GetFilePath(storedFileName);

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (IOException ex)
        {
            throw new InvalidOperationException($"Failed to delete file: {ex.Message}", ex);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> FileExistsAsync(string storedFileName)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(storedFileName), "Stored file name cannot be empty");

        string filePath = GetFilePath(storedFileName);
        return Task.FromResult(File.Exists(filePath));
    }

    /// <inheritdoc/>
    public string GetFilePath(string storedFileName)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(storedFileName), "Stored file name cannot be empty");

        // Prevent directory traversal attacks
        string fileName = Path.GetFileName(storedFileName);
        return Path.Combine(_storagePath, fileName);
    }

    /// <inheritdoc/>
    public void ValidateFile(string fileName, string contentType, long fileSizeBytes)
    {
        Debug.Assert(!string.IsNullOrWhiteSpace(fileName), "File name cannot be empty");
        Debug.Assert(!string.IsNullOrWhiteSpace(contentType), "Content type cannot be empty");

        // Validate file size
        if (fileSizeBytes <= 0)
        {
            throw new BadRequestException("File is empty");
        }

        if (fileSizeBytes > MAX_FILE_SIZE_BYTES)
        {
            throw new BadRequestException($"File size exceeds maximum allowed size of {MAX_FILE_SIZE_BYTES / (1024 * 1024)} MB");
        }

        // Validate content type
        if (!ALLOWED_CONTENT_TYPES.Contains(contentType))
        {
            throw new BadRequestException($"File type '{contentType}' is not allowed. Only image files are permitted.");
        }

        // Validate file extension
        string extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !ALLOWED_EXTENSIONS.Contains(extension))
        {
            throw new BadRequestException($"File extension '{extension}' is not allowed. Only image files are permitted.");
        }
    }
}
