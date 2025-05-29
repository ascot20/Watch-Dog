using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface IProjectService
{
    Task<int> CreateProjectAsync(string title, string description);
    Task<Project?> GetProjectAsync(int projectId);
    Task<IEnumerable> GetAllProjectsAsync();
    Task<bool> UpdateProjectAsync(
        int projectId, 
        ProjectStatus status);
    Task<bool> DeleteProjectAsync(int projectId);
    Task<bool> AddUserToProjectAsync(int projectId, int userId);
    Task<bool> RemoveUserFromProjectAsync(int projectId, int userId);
    Task<bool> ProjectExistsAsync(int projectId);
}