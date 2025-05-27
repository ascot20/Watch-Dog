using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface IProjectService
{
    Task<int> CreateProjectAsync(Project project);
    Task<Project?> GetProjectAsync(int projectId);
    Task<IEnumerable> GetAllProjectsAsync();
    Task<bool> UpdateProjectAsync(Project project);
    Task<bool> DeleteProjectAsync(int projectId);
    Task<bool> AddUserToProjectAsync(int projectId, int userId);
    Task<bool> RemoveUserFromProjectAsync(int projectId, int userId);
}