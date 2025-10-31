namespace DocsAndPlannings.Core.Exceptions.Planning;

/// <summary>
/// Exception thrown when a circular hierarchy is detected in work items
/// </summary>
public class CircularHierarchyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the CircularHierarchyException class with a specified error message
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    public CircularHierarchyException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the CircularHierarchyException class with a specified error message and inner exception
    /// </summary>
    /// <param name="message">The message that describes the error</param>
    /// <param name="innerException">The exception that is the cause of the current exception</param>
    public CircularHierarchyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
