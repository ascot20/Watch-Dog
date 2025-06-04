using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Services;

public interface ISubtaskService
{
    Task<int> CreateSubtaskAsync(string description, int taskId, int creatorId);
    Task<SubTask?> GetSubtaskAsync(int subTaskId);
    Task<IEnumerable<SubTask>> GetSubtasksByTaskIdAsync(int taskId);
    Task<bool> UpdateSubtaskAsync(int subTaskId, bool isCompleted);
    Task<bool> DeleteSubtaskAsync(int subTaskId);
    Task<bool> SubtaskExistsAsync(int subTaskId);
}