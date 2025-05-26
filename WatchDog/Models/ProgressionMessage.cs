using System;

namespace WatchDog.Models;

public class ProgressionMessage:Message
{
    //Foreign keys
    public int SubTaskId { get; set; }
    
    //Navigation properties
    public SubTask? SubTask { get; set; }
    public User? Author { get; set; }
}