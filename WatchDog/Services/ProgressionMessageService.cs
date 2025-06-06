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


        var newMessage = new ProgressionMessage
        {
            Content = message,
            TaskId = taskId,
            AuthorId = creatorId,
        };

        return await _progressionMessageRepository.CreateAsync(newMessage);
    }

    public Task<IEnumerable<ProgressionMessage>> GetByTaskIdAsync(int taskId)
    {
        var messages = _progressionMessageRepository.GetByTaskIdAsync(taskId);
        return messages;
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