using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IUserProjectRepository
{
    Task<bool> AddAsync(int userId, int projectId);
    Task<bool> RemoveAsync(int userId, int projectId);
    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(int userId);
    Task<IEnumerable<User>> GetUsersByProjectIdAsync(int projectId);
}