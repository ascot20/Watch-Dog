using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface ISubtaskService
{
    Task<int> CreateSubtaskAsync(SubTask subTask);
    Task<SubTask?> GetSubtaskAsync(int subTaskId);
    Task<bool> UpdateSubtaskAsync(SubTask subTask);
    Task<bool> DeleteSubtaskAsync(int subTaskId);
}