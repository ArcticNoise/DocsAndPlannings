using System.Diagnostics;
using DocsAndPlannings.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocsAndPlannings.Web.Controllers;

public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    // GET: /Error
    [Route("Error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index(int? statusCode = null)
    {
        string requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        ErrorViewModel model = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = statusCode ?? 500,
            Title = "Error",
            Message = "An unexpected error occurred while processing your request."
        };

        _logger.LogWarning("Error page displayed - Status: {StatusCode}, RequestId: {RequestId}",
            model.StatusCode, requestId);

        return View(model);
    }

    // GET: /Error/NotFound (404)
    [Route("Error/NotFound")]
    [ActionName("NotFound")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult NotFoundPage()
    {
        string requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        ErrorViewModel model = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = 404,
            Title = "Page Not Found",
            Message = "The page you are looking for could not be found.",
            Details = "The requested URL may have been moved, deleted, or never existed."
        };

        _logger.LogInformation("404 Not Found - RequestId: {RequestId}, Path: {Path}",
            requestId, HttpContext.Request.Path);

        Response.StatusCode = 404;
        return View(model);
    }

    // GET: /Error/Unauthorized (401)
    [Route("Error/Unauthorized")]
    [ActionName("Unauthorized")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult UnauthorizedPage()
    {
        string requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        ErrorViewModel model = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = 401,
            Title = "Unauthorized",
            Message = "You must be logged in to access this page.",
            Details = "Please log in to continue."
        };

        _logger.LogInformation("401 Unauthorized - RequestId: {RequestId}, Path: {Path}",
            requestId, HttpContext.Request.Path);

        Response.StatusCode = 401;
        return View(model);
    }

    // GET: /Error/Forbidden (403)
    [Route("Error/Forbidden")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Forbidden()
    {
        string requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        ErrorViewModel model = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = 403,
            Title = "Access Forbidden",
            Message = "You do not have permission to access this resource.",
            Details = "Please contact an administrator if you believe this is an error."
        };

        _logger.LogWarning("403 Forbidden - RequestId: {RequestId}, Path: {Path}, User: {User}",
            requestId, HttpContext.Request.Path, User.Identity?.Name ?? "Anonymous");

        Response.StatusCode = 403;
        return View(model);
    }

    // GET: /Error/ServerError (500)
    [Route("Error/ServerError")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult ServerError()
    {
        string requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        ErrorViewModel model = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = 500,
            Title = "Server Error",
            Message = "An internal server error occurred.",
            Details = "We're sorry for the inconvenience. Please try again later."
        };

        _logger.LogError("500 Server Error - RequestId: {RequestId}, Path: {Path}",
            requestId, HttpContext.Request.Path);

        Response.StatusCode = 500;
        return View(model);
    }

    // GET: /Error/StatusCode/{code}
    [Route("Error/StatusCode/{code:int}")]
    [ActionName("StatusCode")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult HandleStatusCode(int code)
    {
        string requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        // Redirect to specific error pages for common status codes
        return code switch
        {
            404 => RedirectToAction(nameof(NotFoundPage)),
            401 => RedirectToAction(nameof(UnauthorizedPage)),
            403 => RedirectToAction(nameof(Forbidden)),
            500 => RedirectToAction(nameof(ServerError)),
            _ => Index(code)
        };
    }
}
