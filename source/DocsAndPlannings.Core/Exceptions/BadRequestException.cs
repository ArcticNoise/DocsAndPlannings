namespace DocsAndPlannings.Core.Exceptions;

/// <summary>
/// Exception thrown when a request is invalid or malformed
/// </summary>
public class BadRequestException : Exception
{
    /// <summary>
    /// Initializes a new instance of the BadRequestException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public BadRequestException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the BadRequestException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public BadRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
