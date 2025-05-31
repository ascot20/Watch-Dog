using System.Threading.Tasks;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public interface IProjectRepository: IRepository<Project>
{
    Task<bool> ExistsByTitleAsync(string title);
}