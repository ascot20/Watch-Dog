using System;
using System.Collections.Generic;

namespace WatchDog.Models;

public class TimeLineMessage
{
    public int Id { get; set; }
    public required string Message { get; set; }
    public MessageType Type { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsPinned { get; set; }
    
    //Foreign keys
    public int ProjectId{get;set;}
    public int AuthorId{get;set;}
    
    //Navigation properties
    public Project? Project {get;set;}
    public User? Author {get;set;}
    public ICollection<TimeLineReply> Replies {get;set;} = new List<TimeLineReply>();
}

public enum MessageType
{
    Update,
    Announcement,
    Milestone,
    Question
}