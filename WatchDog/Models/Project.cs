using System;
using System.Collections.Generic;

namespace WatchDog.Models;

public class Project:AuditableEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; }
    
    //Navigation properties
    public ICollection<Task> Tasks {get;set;} = new List<Task>();
    public ICollection<User> ProjectMembers {get;set;} = new List<User>();
    public ICollection<TimeLineMessage> TimeLineMessages {get;set;} = new List<TimeLineMessage>();
    
}

public enum ProjectStatus
{
    NotStarted,
    InProgress,
    Completed,
    Closed
}