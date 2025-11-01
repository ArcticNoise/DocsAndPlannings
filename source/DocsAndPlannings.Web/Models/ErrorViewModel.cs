namespace DocsAndPlannings.Web.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public int StatusCode { get; set; }
    public string Title { get; set; } = "Error";
    public string Message { get; set; } = "An error occurred while processing your request.";
    public string? Details { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
