using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IUserRepository: IRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserWithAssignedTasksAsync(int userId);
    Task<IEnumerable<User>> GetUsersByProjectIdAsync(int projectId);
    Task<bool> AddUserToProjectAsync(int userId, int projectId);
    Task<bool> RemoveUserFromProjectAsync(int userId, int projectId);
    
}