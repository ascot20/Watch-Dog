using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;
using Task = System.Threading.Tasks.Task;

namespace WatchDog.Data.Repositories;

public interface IUserRepository: IRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserWithAssignedTasksAsync(int userId, string taskTable = "Tasks");
    Task<IEnumerable<User>> GetUsersByProjectIdAsync(int projectId, string userPrjectTable = "UserProjects");
    Task UpdatePasswordAsync(int userId, string newPasswordHash);
}