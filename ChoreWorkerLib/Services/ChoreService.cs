using ChoreWorkerLib.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace ChoreWorkerLib.Services
{
    public class ChoreService : IChoreService
    {
        private IList<Chore> _chores;
        public ChoreService()
        {
            _chores = new List<Chore>();
            _chores.Add(new Chore(
                // TODO: make this read from a file or database
                // This is just a bootstrap example
                "d0eba7b0-dba4-4b03-9260-0c074018b5ad",
                "Walk Dog",
                "Take the dog unit for a walk",
                DateTime.Now));
        }

        public Chore GetChoreById(string id)
        {
            return GetChoreByIdAsync(id).Result;
        }

        public Task<Chore> GetChoreByIdAsync(string id)
        {
            return Task.FromResult(_chores.Single(w => Equals(w.Id, id)));
        }

        public Task<IEnumerable<Chore>> GetChoresAsync()
        {
            return Task.FromResult(_chores.AsEnumerable());
        }
    }

    public interface IChoreService
    {
        Chore GetChoreById(string id);
        Task<Chore> GetChoreByIdAsync(string id);
        Task<IEnumerable<Chore>> GetChoresAsync();
    }
}
