using System;

namespace WatchDog.Models;

public class TimeLineReply
{
    public int Id { get; set; }
    public required string Reply { get; set; }
    public DateTime CreatedDate { get; set; }
    
    //Foreign keys
    public int TimeLineMessageId { get; set; }
    public int AuthorId { get; set; }
    
    //Navigation properties
    public TimeLineMessage? TimeLineMessage { get; set; }
    public User? Author { get; set; }
}