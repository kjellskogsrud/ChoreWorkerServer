using ChoreWorkerLib.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;

namespace ChoreWorkerLib.Services
{
    public class WorkerService : IWorkerService
    {
        private IList<Worker> _workers;
        public WorkerService()
        {
            _workers = new List<Worker>();
            _workers.Add(new Worker()
            {
                // TODO: make this read from a file or database
                // This is just a bootstrap example 
                Id = "09755ed2-c194-481d-896d-6090063d11a8",
                Name = "Foo"
            });
        }
        public Worker GetWorkerById(string id)
        {
            return GetWorkerByIdAsync(id).Result;
        }
        public Task<Worker> GetWorkerByIdAsync(string id)
        {
            return Task.FromResult(_workers.Single(w => Equals(w.Id, id)));
        }
        public Task<IEnumerable<Worker>> GetWorkersAsync()
        {
            return Task.FromResult(_workers.AsEnumerable());
        }
    }

    public interface IWorkerService
    {
        Worker GetWorkerById(string id);
        Task<Worker> GetWorkerByIdAsync(string id);
        Task<IEnumerable<Worker>> GetWorkersAsync();
    }
}
