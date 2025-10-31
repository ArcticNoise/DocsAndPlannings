namespace DocsAndPlannings.Core.Exceptions.Planning;

/// <summary>
/// Exception thrown when a duplicate key is detected (Project key, Epic key, or Work Item key)
/// </summary>
public class DuplicateKeyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the DuplicateKeyException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public DuplicateKeyException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateKeyException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public DuplicateKeyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
