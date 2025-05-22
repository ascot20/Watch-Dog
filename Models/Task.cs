using System;
using System.Collections.Generic;

namespace WatchDog.Models;

public class Task
{
    public int Id { get; set; }
    public required string TaskDescription { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int PercentageComplete { get; set; }
    
    //Foreign keys
    public int ProjectId { get; set; }
    public int AssignedUserId { get; set; }
    
    //Navigation properties
    public Project? Project { get; set; }
    public User? AssignedUser { get; set; }
    public ICollection<SubTask> SubTasks {get;set;} = new List<SubTask>();
}