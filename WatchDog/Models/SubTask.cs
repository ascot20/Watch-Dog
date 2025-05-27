using System;
using System.Collections.Generic;

namespace WatchDog.Models;

public class SubTask:AuditableEntity
{
    public required string Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public SubTaskStatus Status { get; set; }
    
    //Foreign keys
    public required int TaskId { get; set; }
    public required int CreatedById { get; set; }
    
    //Navigation properties
    public ICollection<ProgressionMessage> ProgressionMessages {get;set;} = new List<ProgressionMessage>();
}

public enum SubTaskStatus
{
    NotStarted,
    InProgress,
    Completed,
    OnHold,
    Closed
}