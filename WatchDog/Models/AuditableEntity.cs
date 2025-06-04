using System;

namespace WatchDog.Models;

public abstract class AuditableEntity:BaseModel
{
    public DateTime CreatedDate { get; set; }
}