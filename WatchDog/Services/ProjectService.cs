using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserProjectRepository _userProjectRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISubTaskRepository _subTaskRepository;
    private readonly ITimeLineMessageRepository _timeLineMessageRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;


    public ProjectService(
        IProjectRepository projectRepository,
        IUserProjectRepository userProjectRepository,
        IUserRepository userRepositoory,
        ITaskRepository taskRepository,
        ISubTaskRepository subTaskRepository,
        ITimeLineMessageRepository timeLineMessageRepository,
        IAuthorizationService authorizationService)
    {
        _projectRepository = projectRepository;
        _userProjectRepository = userProjectRepository;
        _userRepository = userRepositoory;
        _taskRepository = taskRepository;
        _subTaskRepository = subTaskRepository;
        _timeLineMessageRepository = timeLineMessageRepository;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateProjectAsync(Project project)
    {
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project), "Project cannot be null");
        }

        if (string.IsNullOrWhiteSpace(project.Title))
        {
            throw new ArgumentException("Project title cannot be empty", nameof(project));
        }

        if (!_authorizationService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only admins can create projects");
        }

        try
        {
            project.Status = ProjectStatus.NotStarted;
            project.StartDate = DateTime.UtcNow;

            int projectId = await _projectRepository.CreateAsync(project);

            var timelineMessage = new TimeLineMessage
            {
                Content = $"Project '{project.Title}' has been created",
                Type = MessageType.Announcement,
                IsPinned = true,
                ProjectId = projectId,
                AuthorId = _authorizationService.GetCurrentUserId(),
                CreatedDate = DateTime.UtcNow
            };

            await _timeLineMessageRepository.CreateAsync(timelineMessage);

            return projectId;
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating project: {e.Message}");
        }
    }

    public async Task<Project?> GetProjectAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);

            if (project != null)
            {
                project.Tasks = (await _taskRepository.GetByProjectIdAsync(projectId)).ToList();
                project.TimeLineMessages = (await _timeLineMessageRepository.GetByProjectIdAsync(projectId)).ToList();
                project.ProjectMembers = (await _userProjectRepository.GetUsersByProjectIdAsync(projectId)).ToList();
            }

            return project;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving project: {e.Message}");
        }
    }

    public async Task<bool> UpdateProjectAsync(Project project)
    {
        if (project == null)
        {
            throw new ArgumentNullException(nameof(project), "Project cannot be null");
        }

        if (string.IsNullOrWhiteSpace(project.Title))
        {
            throw new ArgumentException("Project title cannot be empty", nameof(project));
        }

        if (!_authorizationService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can update projects");
        }

        try
        {
            if (project.Status == ProjectStatus.Completed && !project.EndDate.HasValue)
            {
                project.EndDate = DateTime.UtcNow;

                var timelineMessage = new TimeLineMessage
                {
                    Content = $"Project '{project.Title}' has been marked as completed",
                    Type = MessageType.Milestone,
                    IsPinned = true,
                    ProjectId = project.Id,
                    AuthorId = _authorizationService.GetCurrentUserId(),
                    CreatedDate = DateTime.UtcNow
                };

                await _timeLineMessageRepository.CreateAsync(timelineMessage);
            }

            return await _projectRepository.UpdateAsync(project);
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating project: {e.Message}");
        }
    }

    public async Task<bool> DeleteProjectAsync(int projectId)
    {
        if (!_authorizationService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can delete projects");
        }

        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                return false;
            }

            await _projectRepository.DeleteAsync(projectId);
            return true;
        }
        catch (Exception e)
        {
            throw new Exception($"Error deleting project: {e.Message}");
        }
    }


    public async Task<bool> AddUserToProjectAsync(int projectId, int userId)
    {
        if (!_authorizationService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only admins can add users to projects");
        }

        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {projectId} does not exist");
            }

            bool result = await _userProjectRepository.AddAsync(userId, projectId);

            if (result)
            {
                var addedUser = await _userRepository.GetByIdAsync(userId);

                if (addedUser != null)
                {
                    var timelineMessage = new TimeLineMessage
                    {
                        Content = $"{addedUser.Username} has been added to the project",
                        Type = MessageType.Update,
                        IsPinned = false,
                        ProjectId = projectId,
                        AuthorId = _authorizationService.GetCurrentUserId(),
                        CreatedDate = DateTime.UtcNow
                    };

                    await _timeLineMessageRepository.CreateAsync(timelineMessage);
                }
                else
                {
                    var timelineMessage = new TimeLineMessage
                    {
                        Content = $"A new team member has been added to the project",
                        Type = MessageType.Update,
                        IsPinned = false,
                        ProjectId = projectId,
                        AuthorId = _authorizationService.GetCurrentUserId(),
                        CreatedDate = DateTime.UtcNow
                    };

                    await _timeLineMessageRepository.CreateAsync(timelineMessage);
                }
            }

            return result;
        }
        catch (Exception e)
        {
            throw new Exception($"Error adding user to project: {e.Message}");
        }
    }

    public async Task<bool> RemoveUserFromProjectAsync(int projectId, int userId)
    {
        if (!_authorizationService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can remove users from projects");
        }

        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {projectId} does not exist");
            }

            var userTasks = (await _taskRepository.GetByAssignedUserIdAsync(userId))
                .Where(t => t.ProjectId == projectId);

            foreach (var task in userTasks)
            {
                // Reassign to project creator
                task.AssignedUserId = _authorizationService.GetCurrentUserId();
                await _taskRepository.UpdateAsync(task);
            }

            // Remove the user from the project
            bool result = await _userProjectRepository.RemoveAsync(projectId, userId);

            if (result)
            {
                var timelineMessage = new TimeLineMessage
                {
                    Content = $"A team member has been removed from the project",
                    Type = MessageType.Announcement,
                    IsPinned = false,
                    ProjectId = projectId,
                    AuthorId = _authorizationService.GetCurrentUserId(),
                    CreatedDate = DateTime.UtcNow
                };

                await _timeLineMessageRepository.CreateAsync(timelineMessage);
            }

            return result;
        }
        catch (Exception e)
        {
            throw new Exception($"Error removing user from project: {e.Message}");
        }
    }

    public async Task<IEnumerable> GetAllProjectsAsync()
    {
        try
        {
            var projects = await _projectRepository.GetAllAsync();
            return projects;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving all projects: {e.Message}");
        }
    }
}