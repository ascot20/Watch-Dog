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

    public async Task<int> CreateMessageAsync(string message, int subTaskId, int creatorId)
    {
        this.ValidateMessage(message);

        try
        {
            var newMessage = new ProgressionMessage
            {
                Content = message,
                SubTaskId = subTaskId,
                AuthorId = creatorId,
            };

            return await _progressionMessageRepository.CreateAsync(newMessage);
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating progression message: {e.Message}", e);
        }
    }

    public Task<IEnumerable<ProgressionMessage>> GetBySubTaskIdAsync(int subTaskId)
    {
        try
        {
            var messages = _progressionMessageRepository.GetBySubTaskIdAsync(subTaskId);
            return messages;
        }
        catch (Exception e)
        {
          throw new Exception($"Error retrieving progression messages for subtask ID {subTaskId}: {e.Message}", e); 
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