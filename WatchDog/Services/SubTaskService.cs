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

    public SubTaskService(
        ISubTaskRepository subtaskRepository
    )
    {
        _subtaskRepository = subtaskRepository;
    }

    public async Task<int> CreateSubtaskAsync(string description, int taskId, int creatorId)
    {
        Helper.ValidateString(description);
        Helper.ValidateId(taskId);
        Helper.ValidateId(creatorId);

        var newSubTask = new SubTask
        {
            Description = description,
            CreatedById = creatorId,
            TaskId = taskId
        };

        int subtaskId = await _subtaskRepository.CreateAsync(newSubTask);

        return subtaskId;
    }

    public async Task<SubTask?> GetSubtaskAsync(int subTaskId)
    {
        Helper.ValidateId(subTaskId);
        var subtask = await _subtaskRepository.GetByIdAsync(subTaskId);

        return subtask;
    }

    public async Task<IEnumerable<SubTask>> GetSubtasksByTaskIdAsync(int taskId)
    {
        Helper.ValidateId(taskId);
        var subtasks = await _subtaskRepository.GetByTaskIdAsync(taskId);
        return subtasks;
    }

    public async Task<bool> UpdateSubtaskAsync(int subTaskId, string description, SubTaskStatus status)
    {
        Helper.ValidateId(subTaskId);

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

    public async Task<bool> DeleteSubtaskAsync(int subTaskId)
    {
        Helper.ValidateId(subTaskId);

        await _subtaskRepository.DeleteAsync(subTaskId);

        return true;
    }

    public async Task<bool> SubtaskExistsAsync(int subTaskId)
    {
        Helper.ValidateId(subTaskId);

        var subtask = await _subtaskRepository.GetByIdAsync(subTaskId);
        return subtask != null;
    }
}