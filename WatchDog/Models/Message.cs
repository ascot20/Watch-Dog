namespace WatchDog.Models;

public abstract class Message: AuditableEntity
{
    public required string Content { get; set; }
    public int AuthorId { get; set; }
    
    public User? Author { get; set; }
}