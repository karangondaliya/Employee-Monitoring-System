namespace Employee_Monitoring_System.Models;

public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed,
    Cancelled
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

public class TaskModel
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime DueDate { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public string AssignedTo { get; set; }
    public string AssignedBy { get; set; }

    // UI helper properties
    public string StatusText => Status.ToString();
    public Color StatusColor => Status switch
    {
        TaskStatus.NotStarted => Colors.Gray,
        TaskStatus.InProgress => Colors.Blue,
        TaskStatus.Completed => Colors.Green,
        TaskStatus.Cancelled => Colors.Red,
        _ => Colors.Black
    };

    public Color DueDateColor => DueDate.Date < DateTime.Now.Date ? Colors.Red :
                                 DueDate.Date == DateTime.Now.Date ? Colors.Orange : Colors.Black;

    public Color PriorityColor => Priority switch
    {
        TaskPriority.Low => Colors.Green,
        TaskPriority.Medium => Colors.Blue,
        TaskPriority.High => Colors.Orange,
        TaskPriority.Critical => Colors.Red,
        _ => Colors.Black
    };
}