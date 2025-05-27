using System;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class ProgressionMessageService: IProgressionMessageService
{
    private readonly IProgressionMessageRepository _progressionMessageRepository;
    private readonly ISubTaskRepository _subTaskRepository;

    public ProgressionMessageService(
        IProgressionMessageRepository progressionMessageRepository,
        ISubTaskRepository subTaskRepository)
    {
        _progressionMessageRepository = progressionMessageRepository;
        _subTaskRepository = subTaskRepository;
    }
    
    public async Task<int> CreateMessageAsync(ProgressionMessage progressionMessage)
    {
        if (progressionMessage == null)
        {
            throw new ArgumentNullException(nameof(progressionMessage), "Progression message cannot be null");
        }

        if (string.IsNullOrWhiteSpace(progressionMessage.Content))
        {
            throw new ArgumentException("Message content cannot be empty", nameof(progressionMessage));
        }

        try
        {
            var subtask = await _subTaskRepository.GetByIdAsync(progressionMessage.SubTaskId);
            if (subtask == null)
            {
                throw new ArgumentException($"Subtask with ID {progressionMessage.SubTaskId} does not exist");
            }

            progressionMessage.CreatedDate = DateTime.UtcNow;

            return await _progressionMessageRepository.CreateAsync(progressionMessage);
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating progression message: {e.Message}", e);
        }
    }


}