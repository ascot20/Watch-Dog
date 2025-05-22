using System;

namespace WatchDog.Models;

public class UserProject
{
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public DateTime JoinedDate { get; set; }
    
    //Navigation properties
    public User? User { get; set; }
    public Project? Project { get; set; }
}