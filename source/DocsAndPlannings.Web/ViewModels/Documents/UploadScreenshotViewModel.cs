using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DocsAndPlannings.Web.ViewModels.Documents;

/// <summary>
/// View model for uploading a screenshot or attachment to a document
/// </summary>
public sealed class UploadScreenshotViewModel
{
    /// <summary>
    /// Gets or sets the document ID to attach the file to
    /// </summary>
    [Required(ErrorMessage = "Document ID is required")]
    public int DocumentId { get; set; }

    /// <summary>
    /// Gets or sets the file to upload
    /// </summary>
    [Required(ErrorMessage = "Please select a file to upload")]
    [Display(Name = "Screenshot/Image")]
    public IFormFile? File { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed file size in bytes (10 MB default)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the allowed file extensions
    /// </summary>
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];

    /// <summary>
    /// Gets the maximum file size in MB for display
    /// </summary>
    public decimal MaxFileSizeMB => MaxFileSizeBytes / (1024m * 1024m);

    /// <summary>
    /// Gets the allowed extensions as a comma-separated string
    /// </summary>
    public string AllowedExtensionsDisplay => string.Join(", ", AllowedExtensions);
}
