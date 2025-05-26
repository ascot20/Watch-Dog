using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WatchDog.Data.Factories;
using WatchDog.Models;

namespace WatchDog.Data.Repositories;

public class SubTaskRepository: Repository<SubTask>, ISubTaskRepository
{
   public SubTaskRepository(IDbConnectionFactory dbConnectionFactory)
       : base(dbConnectionFactory, "SubTasks")
   {
   }

   public override Task<int> CreateAsync(SubTask entity)
   {
       throw new NotImplementedException();
   }

   public Task<IEnumerable<SubTask>> GetByTaskIdAsync(int taskId)
   {
       throw new NotImplementedException();
   }

   public Task<bool> UpdateStatusAsync(int subTaskId, SubTaskStatus newStatus)
   {
       throw new NotImplementedException();
   }

   public Task<int> GetCountForTaskAsync(int taskId)
   {
       throw new NotImplementedException();
   }
}