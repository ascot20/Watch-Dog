using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IProjectRepository: IRepository<Project>
{
    Task<Project?> GetAllProjectsAsync();
    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(int userId);
    Task<Project?> GetProjectWithUsersAsync(int projectId);
    Task<Project?> GetProjectWithTasksAsync(int projectId);
    Task<Project?> GetCompleteProjectAsync(int projectId);
    Task<Project?> GetUncompletedProjectAsync(int projectId);
    Task<IEnumerable<Project>> SearchProjectsByTitleAsync(string titleTerm);
    Task<IEnumerable<Project>> SearchProjectsByMemberNameAsync(string memberNameTerm);
    Task<IEnumerable<Project>> SearchProjectsByDateAsync(DateTime dateTerm);
}