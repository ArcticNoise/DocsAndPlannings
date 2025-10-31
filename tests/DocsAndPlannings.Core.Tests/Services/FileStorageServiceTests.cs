using DocsAndPlannings.Core.Exceptions;
using DocsAndPlannings.Core.Services;

namespace DocsAndPlannings.Core.Tests.Services;

public sealed class FileStorageServiceTests : IDisposable
{
    private readonly IFileStorageService m_FileStorageService;
    private readonly List<string> m_CreatedFiles = new List<string>();
    private readonly string m_TestStorageDirectory;

    public FileStorageServiceTests()
    {
        m_FileStorageService = new FileStorageService();
        m_TestStorageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "screenshots");

        // Ensure test directory exists
        if (!Directory.Exists(m_TestStorageDirectory))
        {
            Directory.CreateDirectory(m_TestStorageDirectory);
        }
    }

    public void Dispose()
    {
        // Clean up test files
        foreach (string storedFileName in m_CreatedFiles)
        {
            try
            {
                string filePath = m_FileStorageService.GetFilePath(storedFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public async Task SaveFileAsync_SavesFile_Successfully()
    {
        byte[] fileContent = "test image content"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);

        string storedFileName = await m_FileStorageService.SaveFileAsync(stream, "test.png", "image/png");
        m_CreatedFiles.Add(storedFileName);

        Assert.NotNull(storedFileName);
        Assert.NotEmpty(storedFileName);
        Assert.EndsWith(".png", storedFileName);

        bool fileExists = await m_FileStorageService.FileExistsAsync(storedFileName);
        Assert.True(fileExists);
    }

    [Fact]
    public async Task SaveFileAsync_GeneratesUniqueFileNames_ForMultipleSaves()
    {
        byte[] fileContent = "test"u8.ToArray();

        using MemoryStream stream1 = new MemoryStream(fileContent);
        string storedFileName1 = await m_FileStorageService.SaveFileAsync(stream1, "test.png", "image/png");
        m_CreatedFiles.Add(storedFileName1);

        using MemoryStream stream2 = new MemoryStream(fileContent);
        string storedFileName2 = await m_FileStorageService.SaveFileAsync(stream2, "test.png", "image/png");
        m_CreatedFiles.Add(storedFileName2);

        Assert.NotEqual(storedFileName1, storedFileName2);
    }

    [Fact]
    public async Task GetFileAsync_ReturnsFile_Successfully()
    {
        byte[] fileContent = "test image content"u8.ToArray();
        using MemoryStream saveStream = new MemoryStream(fileContent);
        string storedFileName = await m_FileStorageService.SaveFileAsync(saveStream, "test.png", "image/png");
        m_CreatedFiles.Add(storedFileName);

        (Stream fileStream, string contentType) = await m_FileStorageService.GetFileAsync(storedFileName);

        Assert.NotNull(fileStream);
        Assert.Equal("image/png", contentType);

        using MemoryStream ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        byte[] retrievedContent = ms.ToArray();
        Assert.Equal(fileContent, retrievedContent);

        fileStream.Dispose();
    }

    [Fact]
    public async Task GetFileAsync_ReturnsCorrectContentType_ForDifferentExtensions()
    {
        Dictionary<string, string> testCases = new Dictionary<string, string>
        {
            { "test.jpg", "image/jpeg" },
            { "test.jpeg", "image/jpeg" },
            { "test.png", "image/png" },
            { "test.gif", "image/gif" },
            { "test.webp", "image/webp" },
            { "test.bmp", "image/bmp" }
        };

        foreach (KeyValuePair<string, string> testCase in testCases)
        {
            byte[] content = "test"u8.ToArray();
            using MemoryStream stream = new MemoryStream(content);
            string storedFileName = await m_FileStorageService.SaveFileAsync(stream, testCase.Key, testCase.Value);
            m_CreatedFiles.Add(storedFileName);

            (Stream fileStream, string contentType) = await m_FileStorageService.GetFileAsync(storedFileName);
            fileStream.Dispose();

            Assert.Equal(testCase.Value, contentType);
        }
    }

    [Fact]
    public async Task GetFileAsync_ThrowsNotFoundException_WhenFileDoesNotExist()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            async () => await m_FileStorageService.GetFileAsync("nonexistent-file.png"));
    }

    [Fact]
    public async Task DeleteFileAsync_DeletesFile_Successfully()
    {
        byte[] fileContent = "test"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);
        string storedFileName = await m_FileStorageService.SaveFileAsync(stream, "test.png", "image/png");

        bool existsBeforeDelete = await m_FileStorageService.FileExistsAsync(storedFileName);
        Assert.True(existsBeforeDelete);

        await m_FileStorageService.DeleteFileAsync(storedFileName);

        bool existsAfterDelete = await m_FileStorageService.FileExistsAsync(storedFileName);
        Assert.False(existsAfterDelete);
    }

    [Fact]
    public async Task DeleteFileAsync_DoesNotThrow_WhenFileDoesNotExist()
    {
        await m_FileStorageService.DeleteFileAsync("nonexistent-file.png");
        // Should not throw
    }

    [Fact]
    public async Task FileExistsAsync_ReturnsTrue_WhenFileExists()
    {
        byte[] fileContent = "test"u8.ToArray();
        using MemoryStream stream = new MemoryStream(fileContent);
        string storedFileName = await m_FileStorageService.SaveFileAsync(stream, "test.png", "image/png");
        m_CreatedFiles.Add(storedFileName);

        bool exists = await m_FileStorageService.FileExistsAsync(storedFileName);

        Assert.True(exists);
    }

    [Fact]
    public async Task FileExistsAsync_ReturnsFalse_WhenFileDoesNotExist()
    {
        bool exists = await m_FileStorageService.FileExistsAsync("nonexistent-file.png");

        Assert.False(exists);
    }

    [Fact]
    public void GetFilePath_ReturnsCorrectPath()
    {
        string storedFileName = "test-guid.png";

        string filePath = m_FileStorageService.GetFilePath(storedFileName);

        Assert.NotNull(filePath);
        Assert.Contains("screenshots", filePath);
        Assert.EndsWith("test-guid.png", filePath);
    }

    [Fact]
    public void GetFilePath_PreventsDirectoryTraversal()
    {
        string maliciousFileName = "../../../etc/passwd";

        string filePath = m_FileStorageService.GetFilePath(maliciousFileName);

        Assert.NotNull(filePath);
        Assert.Contains("screenshots", filePath);
        Assert.DoesNotContain("..", filePath);
        Assert.EndsWith("passwd", filePath);
    }

    [Fact]
    public void ValidateFile_AcceptsValidImageFile()
    {
        m_FileStorageService.ValidateFile("test.png", "image/png", 1024);
        // Should not throw
    }

    [Fact]
    public void ValidateFile_AcceptsAllAllowedImageTypes()
    {
        string[] allowedTypes = ["image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp", "image/bmp"];
        string[] allowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];

        for (int i = 0; i < allowedTypes.Length; i++)
        {
            m_FileStorageService.ValidateFile($"test{allowedExtensions[i]}", allowedTypes[i], 1024);
            // Should not throw
        }
    }

    [Fact]
    public void ValidateFile_ThrowsBadRequestException_WhenFileSizeIsZero()
    {
        BadRequestException exception = Assert.Throws<BadRequestException>(
            () => m_FileStorageService.ValidateFile("test.png", "image/png", 0));

        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateFile_ThrowsBadRequestException_WhenFileSizeExceedsMaximum()
    {
        long oversizedFile = 11 * 1024 * 1024; // 11 MB (max is 10 MB)

        BadRequestException exception = Assert.Throws<BadRequestException>(
            () => m_FileStorageService.ValidateFile("test.png", "image/png", oversizedFile));

        Assert.Contains("exceeds", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateFile_ThrowsBadRequestException_WhenContentTypeNotAllowed()
    {
        BadRequestException exception = Assert.Throws<BadRequestException>(
            () => m_FileStorageService.ValidateFile("test.pdf", "application/pdf", 1024));

        Assert.Contains("not allowed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateFile_ThrowsBadRequestException_WhenFileExtensionNotAllowed()
    {
        BadRequestException exception = Assert.Throws<BadRequestException>(
            () => m_FileStorageService.ValidateFile("test.txt", "image/png", 1024));

        Assert.Contains("not allowed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateFile_ThrowsBadRequestException_WhenFileHasNoExtension()
    {
        BadRequestException exception = Assert.Throws<BadRequestException>(
            () => m_FileStorageService.ValidateFile("testfile", "image/png", 1024));

        Assert.Contains("not allowed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateFile_IsCaseInsensitive_ForContentType()
    {
        m_FileStorageService.ValidateFile("test.png", "IMAGE/PNG", 1024);
        m_FileStorageService.ValidateFile("test.jpg", "Image/Jpeg", 1024);
        // Should not throw
    }

    [Fact]
    public void ValidateFile_IsCaseInsensitive_ForFileExtension()
    {
        m_FileStorageService.ValidateFile("test.PNG", "image/png", 1024);
        m_FileStorageService.ValidateFile("test.JPG", "image/jpeg", 1024);
        // Should not throw
    }
}
