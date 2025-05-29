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
    private readonly ITimeLineReplyService _timeLineReplyService;
    private readonly IProjectService _projectService;
    private readonly IAuthorizationService _authorizationService;

    public TimeLineMessageService(
        ITimeLineMessageRepository timeLineMessageRepository,
        ITimeLineReplyService timeLineReplyService,
        IProjectService projectService,
        IAuthorizationService authorizationService)
    {
        _timeLineMessageRepository = timeLineMessageRepository;
        _timeLineReplyService = timeLineReplyService;
        _projectService = projectService;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateMessageAsync(string message,
        int creatorId,
        int projectId,
        MessageType type = MessageType.Question,
        bool isPinned = false)
    {
        this.ValidateMessage(message);

        try
        {
            bool projectExists = await _projectService.ProjectExistsAsync(projectId);

            if (!projectExists)
            {
                throw new ArgumentException($"Project with ID {projectId} does not exist");
            }

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
        catch (Exception e)
        {
            throw new Exception($"Error creating timeline message: {e.Message}", e);
        }
    }

    public async Task<TimeLineMessage?> GetMessageAsync(int messageId)
    {
        try
        {
            var message = await _timeLineMessageRepository.GetByIdAsync(messageId);

            if (message != null)
            {
                var replies = await _timeLineReplyService.GetByMessageIdAsync(messageId);
                message.Replies = replies.ToList();
            }

            return message;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving timeline message: {e.Message}", e);
        }
    }

    public async Task<IEnumerable<TimeLineMessage>> GetMessagesByProjectIdAsync(int projectId)
    {
        try
        {
            var messages = await _timeLineMessageRepository.GetByProjectIdAsync(projectId);
            return messages;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving timeline messages for project ID {projectId}: {e.Message}", e);
        }
    }

    public async Task<bool> UpdateMessageAsync(int messageId, MessageType type, bool isPinned)
    {
        try
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
        catch (Exception e)
        {
            throw new Exception($"Error updating timeline message: {e.Message}", e);
        }
    }

    public async Task<bool> DeleteMessageAsync(int messageId)
    {
        try
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
        catch (Exception e)
        {
            throw new Exception($"Error deleting timeline message: {e.Message}", e);
        }
    }

    public async Task<bool> MessageExistsAsync(int messageId)
    {
        try
        {
            var message = await _timeLineMessageRepository.GetByIdAsync(messageId);
            return message != null;
        }
        catch (Exception e)
        {
           throw new Exception($"Error checking if timeline message exists: {e.Message}", e); 
        }
    }

    private void ValidateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Timeline message content cannot be empty");
        }
    }
}