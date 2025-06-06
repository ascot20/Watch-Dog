using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class TimeLineMessageService : ITimeLineMessageService
{
    private readonly ITimeLineMessageRepository _timeLineMessageRepository;
    private readonly IAuthorizationService _authorizationService;

    public TimeLineMessageService(
        ITimeLineMessageRepository timeLineMessageRepository,
        IAuthorizationService authorizationService)
    {
        _timeLineMessageRepository = timeLineMessageRepository;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateMessageAsync(string message,
        int creatorId,
        int projectId,
        MessageType type = MessageType.Question,
        bool isPinned = false)
    {
        this.ValidateMessage(message);


        var newMessage = new TimeLineMessage
        {
            Content = message,
            Type = type,
            IsPinned = isPinned,
            ProjectId = projectId,
            AuthorId = creatorId
        };

        return await _timeLineMessageRepository.CreateAsync(newMessage);
    }

    public async Task<TimeLineMessage?> GetMessageAsync(int messageId)
    {
        var message = await _timeLineMessageRepository.GetByIdAsync(messageId);

        return message;
    }

    public async Task<IEnumerable<TimeLineMessage>> GetMessagesByProjectIdAsync(int projectId)
    {
        var messages = await _timeLineMessageRepository.GetByProjectIdAsync(projectId);
        return messages;
    }

    public async Task<bool> UpdateMessageAsync(int messageId, MessageType type, bool isPinned)
    {
        var existingMessage = await _timeLineMessageRepository.GetByIdAsync(messageId);
        if (existingMessage == null)
        {
            return false;
        }

        if (type != existingMessage.Type || isPinned != existingMessage.IsPinned)
        {
            existingMessage.Type = type;
            existingMessage.IsPinned = isPinned;
            await _timeLineMessageRepository.UpdateAsync(existingMessage);
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteMessageAsync(int messageId)
    {
        var message = await _timeLineMessageRepository.GetByIdAsync(messageId);
        if (message == null)
        {
            return false;
        }

        int currentUserId = _authorizationService.GetCurrentUserId();
        bool isAdmin = _authorizationService.IsAdmin();

        if (!isAdmin && message.AuthorId != currentUserId)
        {
            throw new UnauthorizedAccessException($"You are not authorized to delete message with ID {messageId}");
        }

        await _timeLineMessageRepository.DeleteAsync(messageId);
        return true;
    }

    public async Task<bool> MessageExistsAsync(int messageId)
    {
        var message = await _timeLineMessageRepository.GetByIdAsync(messageId);
        return message != null;
    }

    private void ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Timeline message content cannot be empty");
        }
    }
}