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
    private readonly IProgressionMessageService _progressionMessageService;
    private readonly ITimeLineMessageService _timeLineMessageService;
    private readonly IAuthorizationService _authorizationService;

    public TaskService(
        ITaskRepository taskRepository,
        ISubtaskService subtaskService,
        IProgressionMessageService progressionMessageService,
        ITimeLineMessageService timeLineMessageService,
        IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _subtaskService = subtaskService;
        _timeLineMessageService = timeLineMessageService;
        _authorizationService = authorizationService;
        _progressionMessageService = progressionMessageService;
    }

    public async Task<int> CreateTaskAsync(string description, int projectId, int assignedUserId)
    {
        this.ValidateInputs(description);


        if (!_authorizationService.IsAdmin())
        {
            throw new UnauthorizedAccessException("Only administrators can create tasks");
        }

        var newTask = new Task
        {
            TaskDescription = description,
            ProjectId = projectId,
            AssignedUserId = assignedUserId
        };

        int taskId = await _taskRepository.CreateAsync(newTask);

        await _timeLineMessageService.CreateMessageAsync(
            message: $"Task '{description}' has been created",
            type: MessageType.Update,
            isPinned: false,
            projectId: projectId,
            creatorId: _authorizationService.GetCurrentUserId()
        );

        return taskId;
    }

    public async Task<Models.Task?> GetTaskAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);

        if (task != null)
        {
            task.SubTasks = (await _subtaskService.GetSubtasksByTaskIdAsync(taskId)).ToList();
            task.ProgressionMessages = (await _progressionMessageService.GetByTaskIdAsync(taskId)).ToList();
        }

        return task;
    }

    public async Task<bool> UpdateTaskAsync(
        int taskId,
        string? remarks = null,
        int? percentageCompleted = null,
        int? assignedUserId = null)
    {
        var existingTask = await _taskRepository.GetByIdAsync(taskId);
        if (existingTask == null)
        {
            return false;
        }


        if (!string.IsNullOrWhiteSpace(remarks))
        {
            existingTask.Remarks = remarks;
        }


        bool wasJustCompleted = false;
        if (percentageCompleted != existingTask.PercentageComplete && percentageCompleted != null)
        {
            existingTask.PercentageComplete = percentageCompleted.Value;
            wasJustCompleted = percentageCompleted.Value == 100;
        }

        if (assignedUserId != null)
        {
            existingTask.AssignedUserId = assignedUserId.Value;
        }

        bool result = await _taskRepository.UpdateAsync(existingTask);

        if (result && wasJustCompleted)
        {
            // Create a timeline message for task completion
            await _timeLineMessageService.CreateMessageAsync(
                message: $"Task '{existingTask.TaskDescription}' has been completed",
                type: MessageType.Milestone,
                isPinned: false,
                projectId: existingTask.ProjectId,
                creatorId: _authorizationService.GetCurrentUserId()
            );
        }

        return result;
    }

    public async Task<IEnumerable<Task>> GetByAssignedUserIdAsync(int userId)
    {
        var tasks = await _taskRepository.GetByAssignedUserIdAsync(userId);
        return tasks.ToList();
    }

    public async Task<IEnumerable<Task>> GetByProjectIdAsync(int projectId)
    {
        var tasks = await _taskRepository.GetByProjectIdAsync(projectId);

        if (tasks != null)
        {
            var tasksList = tasks.ToList();
            foreach (var task in tasksList)
            {
                task.SubTasks = (await _subtaskService.GetSubtasksByTaskIdAsync(task.Id)).ToList();
                task.ProgressionMessages = (await _progressionMessageService.GetByTaskIdAsync(task.Id)).ToList();
            }

            return tasksList;
        }

        return Enumerable.Empty<Task>();
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null)
        {
            return false;
        }

        await _taskRepository.DeleteAsync(taskId);

        await _timeLineMessageService.CreateMessageAsync(
            message: $"Task '{task.TaskDescription}' has been deleted",
            type: MessageType.Update,
            isPinned: false,
            projectId: task.ProjectId,
            creatorId: _authorizationService.GetCurrentUserId());

        return true;
    }

    public async Task<bool> TaskExistsAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        return task != null;
    }

    private void ValidateInputs(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Task description cannot be empty");
        }
    }
}