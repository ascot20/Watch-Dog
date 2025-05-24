using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IProjectRepository: IRepository<Project>
{
    Task<IEnumerable<Project>> GetProjectsByUserIdAsync(int userId);
    Task<bool> AddUserToProjectAsync(int projectId, int userId);
    Task<bool> RemoveUserFromProjectAsync(int projectId, int userId);
    Task<Project?> GetProjectWithMembersAsync(int projectId);
    Task<Project?> GetProjectWithTasksAsync(int projectId);
    Task<IEnumerable<Project>> SearchProjectsAsync(
        string? titleTerm = null,
        string? memberNameTerm = null,
        ProjectStatus? status = null
        );
}