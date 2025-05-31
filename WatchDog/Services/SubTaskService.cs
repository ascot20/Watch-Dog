using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class SubTaskService : ISubtaskService
{
    private readonly ISubTaskRepository _subtaskRepository;
    private readonly IProgressionMessageService _progressionMessageService;
    private readonly IAuthorizationService _authorizationService;

    public SubTaskService(
        ISubTaskRepository subtaskRepository,
        IProgressionMessageService progressionMessageService,
        IAuthorizationService authorizationService)
    {
        _subtaskRepository = subtaskRepository;
        _progressionMessageService = progressionMessageService;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateSubtaskAsync(string description, int taskId, int creatorId)
    {
        this.ValidateSubtask(description);

        try
        {
            var newSubTask = new SubTask
            {
                Description = description,
                CreatedById = creatorId,
                TaskId = taskId
            };

            int subtaskId = await _subtaskRepository.CreateAsync(newSubTask);

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
                var progressionMessages = await _progressionMessageService.GetBySubTaskIdAsync(subTaskId);
                subtask.ProgressionMessages = progressionMessages.ToList();
            }

            return subtask;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving subtask: {e.Message}", e);
        }
    }

    public async Task<IEnumerable<SubTask>> GetSubtasksByTaskIdAsync(int taskId)
    {
        try
        {
            var subtasks = await _subtaskRepository.GetByTaskIdAsync(taskId);
            return subtasks;
        }
        catch (Exception e)
        {
           throw new Exception($"Error retrieving subtasks by task ID: {e.Message}", e); 
        }
    }

    public async Task<bool> UpdateSubtaskAsync(int subTaskId, string description, SubTaskStatus status)
    {
        this.ValidateSubtask(description);

        try
        {
            var existingSubtask = await _subtaskRepository.GetByIdAsync(subTaskId);
            if (existingSubtask == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(description) && description != existingSubtask.Description)
            {
                existingSubtask.Description = description;
            }

            if (status != existingSubtask.Status)
            {
                existingSubtask.Status = status;
            }

            return await _subtaskRepository.UpdateAsync(existingSubtask);
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

    public async Task<bool> SubtaskExistsAsync(int subTaskId)
    {
        try
        {
            var subtask = await _subtaskRepository.GetByIdAsync(subTaskId);
            return subtask != null;
        }
        catch (Exception e)
        {
            throw new Exception($"Error checking if subtask exists: {e.Message}", e);
        }
    }

    private void ValidateSubtask(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Subtask description cannot be empty");
        }
    }
}