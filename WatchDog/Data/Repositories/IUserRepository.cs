using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;
using Task = System.Threading.Tasks.Task;

namespace WatchDog.Data.Repositories;

public interface IUserRepository: IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetWithAssignedTasksAsync(int userId);
}