using System;

namespace WatchDog.Models;

public class ProgressionMessage
{
    public int Id { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedDate { get; set; }
    
    //Foreign keys
    public int SubTaskId { get; set; }
    public int AuthorId { get; set; }
    
    //Navigation properties
    public SubTask? SubTask { get; set; }
    public User? Author { get; set; }
}