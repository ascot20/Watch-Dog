using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class TimeLineReplyService : ITimeLineReplyService
{
    private readonly ITimeLineReplyRepository _timeLineReplyRepository;
    private readonly ITimeLineMessageService _timeLineMessageService;

    public TimeLineReplyService(
        ITimeLineReplyRepository timeLineReplyRepository,
        ITimeLineMessageService timeLineMessageService
        )
    {
        _timeLineReplyRepository = timeLineReplyRepository;
        _timeLineMessageService = timeLineMessageService;
    }

    public async Task<int> CreateReplyAsync(string reply, int creatorId, int messageId)
    {
        this.ValidateReply(reply);

        try
        {
            bool messageExists = await _timeLineMessageService.MessageExistsAsync(messageId);
            if (!messageExists)
            {
                throw new ArgumentException("Message does not exist");
            }

            var newReply = new TimeLineReply
            {
                Content = reply,
                AuthorId = creatorId,
                TimeLineMessageId = messageId
            };
            return await _timeLineReplyRepository.CreateAsync(newReply);
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating timeline reply: {e.Message}", e);
        }
    }

    public async Task<IEnumerable<TimeLineReply>> GetByMessageIdAsync(int messageId)
    {
        try
        {
            var replies = await _timeLineReplyRepository.GetByMessageIdAsync(messageId);
            return replies;
        }
        catch (Exception e)
        {
            throw new Exception($"Error retrieving timeline replies: {e.Message}", e);
        }
    }

    private void ValidateReply(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Reply content cannot be empty");
        }
    }
}