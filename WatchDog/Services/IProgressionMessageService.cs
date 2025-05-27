using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface IProgressionMessageService
{
    Task<int> CreateMessageAsync(ProgressionMessage progressionMessage);
}