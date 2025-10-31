namespace DocsAndPlannings.Core.Exceptions;

/// <summary>
/// Exception thrown when a user lacks permission to perform an operation
/// </summary>
public class ForbiddenException : Exception
{
    /// <summary>
    /// Initializes a new instance of the ForbiddenException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public ForbiddenException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ForbiddenException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public ForbiddenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
