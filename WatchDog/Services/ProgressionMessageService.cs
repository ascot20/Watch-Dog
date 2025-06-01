using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class ProgressionMessageService : IProgressionMessageService
{
    private readonly IProgressionMessageRepository _progressionMessageRepository;

    public ProgressionMessageService(
        IProgressionMessageRepository progressionMessageRepository)
    {
        _progressionMessageRepository = progressionMessageRepository;
    }

    public async Task<int> CreateMessageAsync(string message, int taskId, int creatorId)
    {
        this.ValidateMessage(message);

        try
        {
            var newMessage = new ProgressionMessage
            {
                Content = message,
                TaskId = taskId,
                AuthorId = creatorId,
            };

            return await _progressionMessageRepository.CreateAsync(newMessage);
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating progression message: {e.Message}", e);
        }
    }

    public Task<IEnumerable<ProgressionMessage>> GetByTaskIdAsync(int taskId)
    {
        try
        {
            var messages = _progressionMessageRepository.GetByTaskIdAsync(taskId);
            return messages;
        }
        catch (Exception e)
        {
          throw new Exception($"Error retrieving progression messages for subtask ID {taskId}: {e.Message}", e); 
        }
    }

    private void ValidateMessage(string message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Progression message cannot be null");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message content cannot be empty");
        }
    }
}