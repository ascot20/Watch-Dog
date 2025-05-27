using System;
using System.Collections.Generic;

namespace WatchDog.Models;

public class Task:AuditableEntity
{
    public required string TaskDescription { get; set; }
    public string? Remarks { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int PercentageComplete { get; set; }
    
    //Foreign keys
    public required int ProjectId { get; set; }
    public required int AssignedUserId { get; set; }
    
    //Navigation properties
    public ICollection<SubTask> SubTasks {get;set;} = new List<SubTask>();
}