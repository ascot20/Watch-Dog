namespace WatchDog.Models;

public abstract class Message: AuditableEntity
{
    public required string Content { get; set; }
    public required int AuthorId { get; set; }
}