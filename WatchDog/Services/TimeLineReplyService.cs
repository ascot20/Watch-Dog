using System;
using System.Threading.Tasks;
using WatchDog.Data.Repositories;
using WatchDog.Models;

namespace WatchDog.Services;

public class TimeLineReplyService: ITimeLineReplyService
{
    private readonly ITimeLineReplyRepository _timeLineReplyRepository;
    private readonly ITimeLineMessageRepository _timeLineMessageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuthorizationService _authorizationService;

    public TimeLineReplyService(
        ITimeLineReplyRepository timeLineReplyRepository,
        ITimeLineMessageRepository timeLineMessageRepository,
        IUserRepository userRepository,
        IAuthorizationService authorizationService)
    {
        _timeLineReplyRepository = timeLineReplyRepository;
        _timeLineMessageRepository = timeLineMessageRepository;
        _userRepository = userRepository;
        _authorizationService = authorizationService;
    }

    public async Task<int> CreateReplyAsync(TimeLineReply reply)
    {
        if (reply == null)
        {
            throw new ArgumentNullException(nameof(reply), "Timeline reply cannot be null");
        }

        if (string.IsNullOrWhiteSpace(reply.Content))
        {
            throw new ArgumentException("Reply content cannot be empty", nameof(reply));
        }

        try
        {
            var parentMessage = await _timeLineMessageRepository.GetByIdAsync(reply.TimeLineMessageId);
            if (parentMessage == null)
            {
                throw new ArgumentException($"Timeline message with ID {reply.TimeLineMessageId} does not exist");
            }

            var user = await _userRepository.GetByIdAsync(_authorizationService.GetCurrentUserId());
            if (user == null)
            {
                throw new ArgumentException("Invalid user");
            }

            reply.CreatedDate = DateTime.UtcNow;

            return await _timeLineReplyRepository.CreateAsync(reply);
        }
        catch (Exception e)
        {
            throw new Exception($"Error creating timeline reply: {e.Message}", e);
        }
    }
}