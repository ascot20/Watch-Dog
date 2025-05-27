using System;
using System.Linq;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ISubTaskRepository _subTaskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITimeLineMessageRepository _timeLineMessageRepository;
    private readonly IAuthorizationService _authorizationService;

    public TaskService(
        ITaskRepository taskRepository,
        ISubTaskRepository subTaskRepository,
        IUserRepository userRepository,
        IProjectRepository projectRepository,
        ITimeLineMessageRepository timeLineMessageRepository,
        IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _subTaskRepository = subTaskRepository;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _timeLineMessageRepository = timeLineMessageRepository;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateTaskAsync(Models.Task task)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task), "Task cannot be null");
        }

        if (string.IsNullOrWhiteSpace(task.TaskDescription))
        {
            throw new ArgumentException("Task description cannot be empty", nameof(task));
        }

        try
        {
            var project = await _projectRepository.GetByIdAsync(task.ProjectId);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {task.ProjectId} does not exist");
            }

            var user = await _userRepository.GetByIdAsync(task.AssignedUserId);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {task.AssignedUserId} does not exist");
            }

            task.StartDate = task.StartDate ?? DateTime.UtcNow;
            task.PercentageComplete = 0;
            task.CreatedDate = DateTime.UtcNow;

            int taskId = await _taskRepository.CreateAsync(task);

            var timelineMessage = new TimeLineMessage
            {
                Content = $"Task '{task.TaskDescription}' has been created and assigned to {user.Username}",
                Type = MessageType.Update,
                IsPinned = false,
                ProjectId = task.ProjectId,
                AuthorId = _authorizationService.GetCurrentUserId(),
                CreatedDate = DateTime.UtcNow
            };

            await _timeLineMessageRepository.CreateAsync(timelineMessage);

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
                task.SubTasks = (await _subTaskRepository.GetByTaskIdAsync(taskId)).ToList();
            }

            return task;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving task: {e.Message}", e);
        }
    }

    public async Task<bool> UpdateTaskAsync(Models.Task task)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task), "Task cannot be null");
        }

        if (string.IsNullOrWhiteSpace(task.TaskDescription))
        {
            throw new ArgumentException("Task description cannot be empty", nameof(task));
        }

        try
        {
            var existingTask = await _taskRepository.GetByIdAsync(task.Id);
            if (existingTask == null)
            {
                return false;
            }

            bool wasJustCompleted = task.PercentageComplete == 100 && existingTask.PercentageComplete < 100;

            // If task is now 100% complete and wasn't before, set the completed date
            if (wasJustCompleted && !task.CompletedDate.HasValue)
            {
                task.CompletedDate = DateTime.UtcNow;
            }

            bool result = await _taskRepository.UpdateAsync(task);

            if (result && wasJustCompleted)
            {
                // Create a timeline message for task completion
                var assignedUser = await _userRepository.GetByIdAsync(task.AssignedUserId);
                string username = assignedUser?.Username ?? "A user";

                var timelineMessage = new TimeLineMessage
                {
                    Content = $"Task '{task.TaskDescription}' has been completed by {username}",
                    Type = MessageType.Milestone,
                    IsPinned = false,
                    ProjectId = task.ProjectId,
                    AuthorId = _authorizationService.GetCurrentUserId(),
                    CreatedDate = DateTime.UtcNow
                };

                await _timeLineMessageRepository.CreateAsync(timelineMessage);
            }

            return result;
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating task: {e.Message}", e);
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

            var timelineMessage = new TimeLineMessage
            {
                Content = $"Task '{task.TaskDescription}' has been deleted",
                Type = MessageType.Update,
                IsPinned = false,
                ProjectId = task.ProjectId,
                AuthorId = _authorizationService.GetCurrentUserId(),
                CreatedDate = DateTime.UtcNow
            };

            await _timeLineMessageRepository.CreateAsync(timelineMessage);

            return true;
        }
        catch (Exception e)
        {
            throw new Exception($"Error deleting task: {e.Message}", e);
        }
    }
}