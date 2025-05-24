using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;
using Task = System.Threading.Tasks.Task;

namespace WatchDog.Data.Repositories;

public interface IUserRepository: IRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserWithAssignedTasksAsync(int userId);
    Task<IEnumerable<User>> GetUsersByProjectIdAsync(int projectId);
}