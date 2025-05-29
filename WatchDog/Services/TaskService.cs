using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;
using Task = WatchDog.Models.Task;

namespace WatchDog.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ISubtaskService _subtaskService;
    private readonly IUserService _userService;
    private readonly IProjectService _projectService;
    private readonly ITimeLineMessageService _timeLineMessageService;
    private readonly IAuthorizationService _authorizationService;

    public TaskService(
        ITaskRepository taskRepository,
        ISubtaskService subtaskService,
        IUserService userService,
        IProjectService projectService,
        ITimeLineMessageService timeLineMessageService,
        IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _subtaskService = subtaskService;
        _userService = userService;
        _projectService = projectService;
        _timeLineMessageService = timeLineMessageService;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateTaskAsync(string description, int projectId, int assignedUserId)
    {
        this.ValidateInputs(description);

        try
        {
            if (!_authorizationService.IsAdmin())
            {
                throw new UnauthorizedAccessException("Only administrators can create tasks");
            }

            bool projectExists = await _projectService.ProjectExistsAsync(projectId);
            if (!projectExists)
            {
                throw new ArgumentException($"Project with ID {projectId} does not exist");
            }

            var user = await _userService.GetUserAsync(assignedUserId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {assignedUserId} does not exist");
            }

            var newTask = new Task
            {
                TaskDescription = description,
                ProjectId = projectId,
                AssignedUserId = assignedUserId
            };

            int taskId = await _taskRepository.CreateAsync(newTask);

            await _timeLineMessageService.CreateMessageAsync(
                message: $"Task '{description}' has been created and assigned to {user.Username}",
                type: MessageType.Update,
                isPinned: false,
                projectId: projectId,
                creatorId: _authorizationService.GetCurrentUserId()
            );

            return taskId;
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating task: {e.Message}", e);
        }
    }

    public async Task<Models.Task?> GetTaskAsync(int taskId)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(taskId);

            if (task != null)
            {
                task.SubTasks = (await _subtaskService.GetSubtasksByTaskIdAsync(taskId)).ToList();
            }

            return task;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving task: {e.Message}", e);
        }
    }

    public async Task<bool> UpdateTaskAsync(
        int taskId,
        string? description = null,
        string? remarks = null,
        DateTime? startDate = null,
        int? percentageCompleted = null,
        int? assignedUserId = null)
    {
        try
        {
            bool userAuthorized = await _authorizationService.IsUserAuthorizedForTask(taskId);
            if (!userAuthorized)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this task");
            }
            
            var existingTask = await _taskRepository.GetByIdAsync(taskId);
            if (existingTask == null)
            {
                return false;
            }

            bool wasJustCompleted = percentageCompleted == 100 && existingTask.PercentageComplete < 100;

            // If task is now 100% complete and wasn't before, set the completed date
            if (wasJustCompleted)
            {
                existingTask.CompletedDate = DateTime.UtcNow;
            }

            if (!string.IsNullOrWhiteSpace(description))
            {
                existingTask.TaskDescription = description;
            }

            if (!string.IsNullOrWhiteSpace(remarks))
            {
                existingTask.Remarks = remarks;
            }

            if (startDate != existingTask.StartDate)
            {
                existingTask.StartDate = startDate;
            }

            if (percentageCompleted != existingTask.PercentageComplete && percentageCompleted != null)
            {
                existingTask.PercentageComplete = percentageCompleted.Value;
            }

            if (assignedUserId != null)
            {
                existingTask.AssignedUserId = assignedUserId.Value;
            }

            bool result = await _taskRepository.UpdateAsync(existingTask);

            if (result && wasJustCompleted)
            {
                // Create a timeline message for task completion
                var assignedUserName =_authorizationService.GetCurrentUserName(); 

                await _timeLineMessageService.CreateMessageAsync(
                    message:$"Task '{description}' has been completed by {assignedUserName}",
                    type: MessageType.Milestone,
                    isPinned: false,
                    projectId:existingTask.ProjectId,
                    creatorId: _authorizationService.GetCurrentUserId()
                    );
            }

            return result;
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating task: {e.Message}", e);
        } 
    }

    public async Task<IEnumerable<Task>> GetByAssignedUserIdAsync(int userId)
    {
        try
        {
            var tasks = await _taskRepository.GetByAssignedUserIdAsync(userId);
            return tasks.ToList();
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving tasks by assigned user: {e.Message}", e);
        }
    }

    public Task<IEnumerable<Task>> GetByProjectIdAsync(int projectId)
    {
        try
        {
            var tasks = _taskRepository.GetByProjectIdAsync(projectId);
            return tasks;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving tasks by project ID {projectId}: {e.Message}", e);
        }
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                return false;
            }

            await _taskRepository.DeleteAsync(taskId);

            await _timeLineMessageService.CreateMessageAsync(
                message:$"Task '{task.TaskDescription}' has been deleted",
                type: MessageType.Update,
                isPinned: false,
                projectId:task.ProjectId,
                creatorId: _authorizationService.GetCurrentUserId());

            return true;
        }
        catch (Exception e)
        {
            throw new Exception($"Error deleting task: {e.Message}", e);
        }
    }

    public async Task<bool> TaskExistsAsync(int taskId)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            return task != null;
        }
        catch (Exception e)
        {
            throw new Exception($"Error checking if task exists: {e.Message}", e);
        }
    }

    private void ValidateInputs(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Task description cannot be empty");
        }
    }
}