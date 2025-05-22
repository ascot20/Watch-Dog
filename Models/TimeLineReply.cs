using System;

namespace WatchDog.Models;

public class TimeLineReply:Message
{
    //Foreign keys
    public int TimeLineMessageId { get; set; }
    
    //Navigation properties
    public TimeLineMessage? TimeLineMessage { get; set; }
    public User? Author { get; set; }
}