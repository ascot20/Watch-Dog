using System;

namespace WatchDog.Models;

public class UserProject
{
    public required int UserId { get; set; }
    public required int ProjectId { get; set; }
    public DateTime JoinedDate { get; set; }
}