using System.Net;
using System.Text;
using System.Text.Json;
using DocsAndPlannings.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace DocsAndPlannings.Web.Tests.Services;

public sealed class ApiClientTests : IDisposable
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<ApiClient>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly ApiClient _apiClient;

    public ApiClientTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<ApiClient>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:7120")
        };

        _apiClient = new ApiClient(_httpClient, _mockHttpContextAccessor.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_WithValidResponse_ReturnsDeserializedObject()
    {
        // Arrange
        TestDto expectedDto = new() { Id = 1, Name = "Test" };
        string jsonResponse = JsonSerializer.Serialize(expectedDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        // Act
        TestDto? result = await _apiClient.GetAsync<TestDto>("/api/test");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_With404Response_ReturnsNull()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        TestDto? result = await _apiClient.GetAsync<TestDto>("/api/test/999");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task PostAsync_WithValidData_ReturnsDeserializedObject()
    {
        // Arrange
        TestDto inputDto = new() { Id = 0, Name = "New Item" };
        TestDto expectedDto = new() { Id = 1, Name = "New Item" };
        string jsonResponse = JsonSerializer.Serialize(expectedDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        // Act
        TestDto? result = await _apiClient.PostAsync<TestDto>("/api/test", inputDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Name, result.Name);
    }

    [Fact]
    public async Task PutAsync_WithValidData_ReturnsUpdatedObject()
    {
        // Arrange
        TestDto inputDto = new() { Id = 1, Name = "Updated Item" };
        string jsonResponse = JsonSerializer.Serialize(inputDto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        // Act
        TestDto? result = await _apiClient.PutAsync<TestDto>("/api/test/1", inputDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(inputDto.Id, result.Id);
        Assert.Equal(inputDto.Name, result.Name);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ReturnsTrue()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent
            });

        // Act
        bool result = await _apiClient.DeleteAsync("/api/test/1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_With404Response_ReturnsFalse()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        bool result = await _apiClient.DeleteAsync("/api/test/999");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetFileAsync_WithValidEndpoint_ReturnsByteArray()
    {
        // Arrange
        byte[] expectedBytes = Encoding.UTF8.GetBytes("Test file content");

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(expectedBytes)
            });

        // Act
        byte[] result = await _apiClient.GetFileAsync("/api/files/1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task GetAsync_WithAuthToken_IncludesAuthorizationHeader()
    {
        // Arrange
        DefaultHttpContext httpContext = new();

        // Mock the cookie collection
        Mock<IRequestCookieCollection> mockCookies = new();
        mockCookies.Setup(c => c["AuthToken"]).Returns("test-jwt-token");
        httpContext.Request.Cookies = mockCookies.Object;

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        string jsonResponse = JsonSerializer.Serialize(new TestDto { Id = 1, Name = "Test" },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        HttpRequestMessage? capturedRequest = null;
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
            });

        // Act
        await _apiClient.GetAsync<TestDto>("/api/test");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.NotNull(capturedRequest.Headers.Authorization);
        Assert.Equal("Bearer", capturedRequest.Headers.Authorization.Scheme);
        Assert.Equal("test-jwt-token", capturedRequest.Headers.Authorization.Parameter);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    private sealed class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
