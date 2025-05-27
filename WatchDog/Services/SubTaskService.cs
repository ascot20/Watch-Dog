using System;
using System.Linq;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class SubTaskService : ISubtaskService
{
    private readonly ISubTaskRepository _subtaskRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITimeLineMessageRepository _timeLineMessageRepository;
    private readonly IProgressionMessageRepository _progressionMessageRepository;
    private readonly IAuthorizationService _authorizationService;

    public SubTaskService(
        ISubTaskRepository subtaskRepository,
        ITaskRepository taskRepository,
        IUserRepository userRepository,
        ITimeLineMessageRepository timeLineMessageRepository,
        IProgressionMessageRepository progressionMessageRepository,
        IAuthorizationService authorizationService)
    {
        _subtaskRepository = subtaskRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _timeLineMessageRepository = timeLineMessageRepository;
        _progressionMessageRepository = progressionMessageRepository;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateSubtaskAsync(SubTask subTask)
    {
        if (subTask == null)
        {
            throw new ArgumentNullException(nameof(subTask), "Subtask cannot be null");
        }

        if (string.IsNullOrWhiteSpace(subTask.Description))
        {
            throw new ArgumentException("Subtask description cannot be empty", nameof(subTask));
        }

        try
        {
            var task = await _taskRepository.GetByIdAsync(subTask.TaskId);
            if (task == null)
            {
                throw new ArgumentException($"Task with ID {subTask.TaskId} does not exist");
            }

            var user = await _userRepository.GetByIdAsync(subTask.CreatedById);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {subTask.CreatedById} does not exist");
            }

            bool isAuthorized = await _authorizationService.IsUserAuthorizedForTask(subTask.TaskId);
            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException(
                    $"You are not authorized to create subtasks for task with ID {subTask.TaskId}");
            }

            subTask.CreatedDate = DateTime.UtcNow;

            int subtaskId = await _subtaskRepository.CreateAsync(subTask);

            var timelineMessage = new TimeLineMessage
            {
                Content = $"Subtask '{subTask.Description}' has been created for task '{task.TaskDescription}'",
                Type = MessageType.Update,
                IsPinned = false,
                ProjectId = task.ProjectId,
                AuthorId = _authorizationService.GetCurrentUserId(),
                CreatedDate = DateTime.UtcNow
            };

            await _timeLineMessageRepository.CreateAsync(timelineMessage);

            return subtaskId;
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating subtask: {e.Message}", e);
        }
    }

    public async Task<SubTask?> GetSubtaskAsync(int subTaskId)
    {
        try
        {
            var subtask = await _subtaskRepository.GetByIdAsync(subTaskId);

            if (subtask != null)
            {
                var progressionMessages = await _progressionMessageRepository.GetBySubTaskIdAsync(subTaskId);
                subtask.ProgressionMessages = progressionMessages.ToList();
            }

            return subtask;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving subtask: {e.Message}", e);
        }
    }

    public async Task<bool> UpdateSubtaskAsync(SubTask subTask)
    {
        if (subTask == null)
        {
            throw new ArgumentNullException(nameof(subTask), "Subtask cannot be null");
        }

        if (string.IsNullOrWhiteSpace(subTask.Description))
        {
            throw new ArgumentException("Subtask description cannot be empty", nameof(subTask));
        }

        try
        {
            var existingSubtask = await _subtaskRepository.GetByIdAsync(subTask.Id);
            if (existingSubtask == null)
            {
                return false;
            }

            var task = await _taskRepository.GetByIdAsync(existingSubtask.TaskId);
            if (task == null)
            {
                throw new Exception($"Parent task for subtask {subTask.Id} not found");
            }

            bool isAuthorized = await _authorizationService.IsUserAuthorizedForTask(existingSubtask.TaskId);
            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException($"You are not authorized to update subtask with ID {subTask.Id}");
            }

            return await _subtaskRepository.UpdateAsync(subTask);
        }
        catch (Exception e)
        {
            throw new Exception($"Error updating subtask: {e.Message}", e);
        }
    }
    
    public async Task<bool> DeleteSubtaskAsync(int subTaskId)
        {
            try
            {
                await _subtaskRepository.DeleteAsync(subTaskId);

                return true;
            }
            catch (Exception e)
            {
                throw new Exception($"Error deleting subtask: {e.Message}", e);
            }
        }

}