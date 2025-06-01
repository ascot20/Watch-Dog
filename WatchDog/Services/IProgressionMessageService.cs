using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface IProgressionMessageService
{
    Task<int> CreateMessageAsync(string message, int taskId, int creatorId);
    Task<IEnumerable<ProgressionMessage>> GetByTaskIdAsync(int taskId);
}