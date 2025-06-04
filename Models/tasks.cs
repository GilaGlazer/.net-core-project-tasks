using webApiProject.Interfaces;

namespace webApiProject.Models;

public class Tasks : IIdentifiable
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty; // כותרת המשימה

    public string? Description { get; set; } // תיאור מפורט

    public DateTime CreatedAt { get; set; } = DateTime.Now; // מתי נוצרה המשימה

    public DateTime? DueDate { get; set; } // תאריך יעד

    public TaskStatus Status { get; set; } = TaskStatus.Pending; // סטטוס

    public int Priority { get; set; } = 1; // עדיפות (1 - נמוכה, 2 - בינונית, 3 - גבוהה)

    public int UserId { get; set; }
}
