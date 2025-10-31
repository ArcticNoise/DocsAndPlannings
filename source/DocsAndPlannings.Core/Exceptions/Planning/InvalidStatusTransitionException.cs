namespace DocsAndPlannings.Core.Exceptions.Planning;

/// <summary>
/// Exception thrown when an invalid status transition is attempted
/// </summary>
public class InvalidStatusTransitionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the InvalidStatusTransitionException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public InvalidStatusTransitionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the InvalidStatusTransitionException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public InvalidStatusTransitionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
