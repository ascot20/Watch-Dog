using System;
using System.Collections.Generic;

namespace WatchDog.Models;

public class SubTask
{
    public int Id { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public SubTaskStatus Status { get; set; }
    
    //Foreign keys
    public int TaskId { get; set; }
    public int CreatedById { get; set; }
    
    //Navigation properties
    public Task? Task { get; set; }
    public User? CreatedBy { get; set; }
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