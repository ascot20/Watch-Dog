using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IProjectRepository: IRepository<Project>
{
    Task<IEnumerable<Project>> GetByUserIdAsync(int userId);
    Task<Project?> GetWithMembersAsync(int projectId);
    Task<Project?> GetWithTasksAsync(int projectId);
}