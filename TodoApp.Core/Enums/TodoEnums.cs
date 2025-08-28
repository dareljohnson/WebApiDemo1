namespace TodoApp.Core
{
    /// <summary>
    /// Enumeration for Todo priority levels
    /// </summary>
    public enum TodoPriority
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
    
    /// <summary>
    /// Enumeration for Todo status
    /// </summary>
    public enum TodoStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }
}
