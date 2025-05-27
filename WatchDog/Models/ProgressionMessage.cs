using System;

namespace WatchDog.Models;

public class ProgressionMessage:Message
{
    //Foreign keys
    public required int SubTaskId { get; set; }
    
}