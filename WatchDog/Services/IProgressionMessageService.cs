using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface IProgressionMessageService
{
    Task<int> CreateMessageAsync(string message, int subTaskId, int creatorId);
    Task<IEnumerable<ProgressionMessage>> GetBySubTaskIdAsync(int subTaskId);
}