using System;
namespace WatchDog.Models;

public class SubTask:AuditableEntity
{
    public required string Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsComplete { get; set; }
    
    //Foreign keys
    public required int TaskId { get; set; }
    public required int CreatedById { get; set; }
}