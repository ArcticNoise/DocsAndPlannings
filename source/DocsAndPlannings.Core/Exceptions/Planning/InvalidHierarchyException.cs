namespace DocsAndPlannings.Core.Exceptions.Planning;

/// <summary>
/// Exception thrown when an invalid hierarchy structure is detected (e.g., subtask depth exceeded)
/// </summary>
public class InvalidHierarchyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the InvalidHierarchyException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public InvalidHierarchyException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the InvalidHierarchyException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public InvalidHierarchyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
