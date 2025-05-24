using System.Collections.Generic;

namespace WatchDog.Models;

public class User: AuditableEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    
    //Navigation properties
    public ICollection<Task> AssignedTasks {get;set;} = new List<Task>();
    public ICollection<UserProject> UserProjects {get;set;} = new List<UserProject>();
}

public enum UserRole
{
    SuperAdmin,
    User
}