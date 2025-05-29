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
    private readonly IUserService _userService;
    private readonly ITimeLineMessageService _timeLineMessageService;
    private readonly ITaskService _taskService;
    private readonly IAuthorizationService _authorizationService;


    public ProjectService(
        IProjectRepository projectRepository,
        IUserProjectRepository userProjectRepository,
        IUserService userService,
        ITaskService taskService,
        ITimeLineMessageService timeLineMessageService,
        IAuthorizationService authorizationService)
    {
        _projectRepository = projectRepository;
        _userProjectRepository = userProjectRepository;
        _userService = userService;
        _taskService = taskService;
        _timeLineMessageService = timeLineMessageService;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateProjectAsync(string title, string description)
    {
        this.ValidateProject(title);
        this.EnsureAdminAccess();

        try
        {
            var newProject = new Project
            {
                Title = title,
                Description = description,
                StartDate = DateTime.UtcNow,
                Status = ProjectStatus.NotStarted
            };

            int projectId = await _projectRepository.CreateAsync(newProject);

            await _timeLineMessageService.CreateMessageAsync(
                message: $"Project '{title}' has been created",
                creatorId: _authorizationService.GetCurrentUserId(),
                type: MessageType.Announcement,
                projectId: projectId,
                isPinned: true
            );
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
                project.Tasks = (await _taskService.GetByProjectIdAsync(projectId)).ToList();
                project.TimeLineMessages =
                    (await _timeLineMessageService.GetMessagesByProjectIdAsync(projectId)).ToList();
                project.ProjectMembers = (await _userProjectRepository.GetUsersByProjectIdAsync(projectId)).ToList();
            }

            return project;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving project: {e.Message}");
        }
    }

    public async Task<bool> UpdateProjectAsync(
        int projectId,
        ProjectStatus status)
    {
        this.EnsureAdminAccess();

        try
        {
            var projectToUpdate = await this.GetProjectOrThrowAsync(projectId);
            if (status == ProjectStatus.Completed)
            {
                await _timeLineMessageService.CreateMessageAsync(
                    projectId: projectId,
                    message: $"Project '{projectToUpdate.Title}' has been marked as completed",
                    type: MessageType.Milestone,
                    isPinned: false,
                    creatorId: _authorizationService.GetCurrentUserId()
                );
            }

            return await _projectRepository.UpdateAsync(projectToUpdate);
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating project: {e.Message}");
        }
    }


    public async Task<bool> DeleteProjectAsync(int projectId)
    {
        this.EnsureAdminAccess();

        try
        {
            await this.GetProjectOrThrowAsync(projectId);

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
        this.EnsureAdminAccess();

        try
        {
            await this.GetProjectOrThrowAsync(projectId);
            bool result = await _userProjectRepository.AddAsync(userId, projectId);

            if (result)
            {
                string addedUsername = await _userService.GetUserNameAsync(userId);
                
                string message = !string.IsNullOrEmpty(addedUsername)
                    ? $"{addedUsername} has been added to the project"
                    : "A new team member has been added to the project";

                await _timeLineMessageService.CreateMessageAsync(
                    projectId: projectId,
                    message: message,
                    type: MessageType.Update,
                    creatorId: _authorizationService.GetCurrentUserId()
                );
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
        this.EnsureAdminAccess();

        try
        {
            await this.GetProjectOrThrowAsync(projectId);

            var userTasks = (await _taskService.GetByAssignedUserIdAsync(userId))
                .Where(t => t.ProjectId == projectId);

            foreach (var task in userTasks)
            {
                // Reassign to project creator
                task.AssignedUserId = _authorizationService.GetCurrentUserId();
                await _taskService.UpdateTaskAsync(taskId:task.Id, assignedUserId:task.AssignedUserId);
            }

            // Remove the user from the project
            bool result = await _userProjectRepository.RemoveAsync(projectId, userId);

            if (result)
            {
                await _timeLineMessageService.CreateMessageAsync(
                    projectId: projectId,
                    message: "A team member has been removed from the project",
                    type: MessageType.Update,
                    creatorId: _authorizationService.GetCurrentUserId()
                );
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

    public async Task<bool> ProjectExistsAsync(int projectId)
    {
        try
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            return project != null;
        }
        catch (Exception e)
        {
            throw new Exception($"Error checking if project exists: {e.Message}");
        }
    }

    private void ValidateProject(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Project title cannot be empty");
        }
    }

    private void EnsureAdminAccess()
    {
        if (!_authorizationService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can perform this operation");
        }
    }

    private async Task<Project> GetProjectOrThrowAsync(int projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
        {
            throw new ArgumentException($"Project with ID {projectId} does not exist");
        }

        return project;
    }
}