using System;
using System.Linq;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class TimeLineMessageService : ITimeLineMessageService
{
    private readonly ITimeLineMessageRepository _timeLineMessageRepository;
    private readonly ITimeLineReplyRepository _timeLineReplyRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAuthorizationService _authorizationService;

    public TimeLineMessageService(
        ITimeLineMessageRepository timeLineMessageRepository,
        ITimeLineReplyRepository timeLineReplyRepository,
        IProjectRepository projectRepository,
        IAuthorizationService authorizationService)
    {
        _timeLineMessageRepository = timeLineMessageRepository;
        _timeLineReplyRepository = timeLineReplyRepository;
        _projectRepository = projectRepository;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateMessageAsync(TimeLineMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Timeline message cannot be null");
        }

        if (string.IsNullOrWhiteSpace(message.Content))
        {
            throw new ArgumentException("Timeline message content cannot be empty", nameof(message));
        }

        try
        {
            var project = await _projectRepository.GetByIdAsync(message.ProjectId);
            if (project == null)
            {
                throw new ArgumentException($"Project with ID {message.ProjectId} does not exist");
            }

            message.CreatedDate = DateTime.UtcNow;

            return await _timeLineMessageRepository.CreateAsync(message);
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
                var replies = await _timeLineReplyRepository.GetByMessageIdAsync(messageId);
                message.Replies = replies.ToList();
            }

            return message;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving timeline message: {e.Message}", e);
        }
    }

    public async Task<bool> UpdateMessageAsync(TimeLineMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message), "Timeline message cannot be null");
        }

        if (string.IsNullOrWhiteSpace(message.Content))
        {
            throw new ArgumentException("Timeline message content cannot be empty", nameof(message));
        }

        try
        {
            var existingMessage = await _timeLineMessageRepository.GetByIdAsync(message.Id);
            if (existingMessage == null)
            {
                return false;
            }

            return await _timeLineMessageRepository.UpdateAsync(message);
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
}