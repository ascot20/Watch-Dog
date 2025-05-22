using System;

namespace WatchDog.Models;

public abstract class AuditableEntity:BaseEntity
{
    public DateTime CreatedDate { get; set; }
}