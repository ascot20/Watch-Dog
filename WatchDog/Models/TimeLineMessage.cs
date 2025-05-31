using System;
using System.Collections.Generic;

namespace WatchDog.Models;

public class TimeLineMessage:Message
{
    public MessageType Type { get; set; }
    public bool IsPinned { get; set; }
    
    //Foreign keys
    public required int ProjectId{get;set;}
    
    //Navigation properties
    public string? AuthorName {get;set;}
}

public enum MessageType
{
    Update,
    Announcement,
    Milestone,
    Question
}