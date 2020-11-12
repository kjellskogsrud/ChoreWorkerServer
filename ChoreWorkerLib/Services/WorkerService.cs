using ChoreWorkerLib.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.IO;

using Newtonsoft.Json;

namespace ChoreWorkerLib.Services
{
    public class WorkerService : IWorkerService
    {
        private IList<Worker> _workers;
        public WorkerService()
        {
            _workers = new List<Worker>();
            DeserializeWorkers();
        }

        public void SerializeWorkers()
        {
            StreamWriter workersJson = new StreamWriter("workers.json");
            workersJson.Write(JsonConvert.SerializeObject(_workers, Formatting.Indented));
            workersJson.Close();
        }

        public void DeserializeWorkers()
        {
            StreamReader workerJson = new StreamReader("workers.json");
            this._workers = JsonConvert.DeserializeObject<List<Worker>>(workerJson.ReadToEnd());
            workerJson.Close();
        }

        public Worker GetWorkerById(string id)
        {
            return GetWorkerByIdAsync(id).Result;
        }

        public Task<Worker> GetWorkerByIdAsync(string id)
        {
            return Task.FromResult(_workers.Single(w => Equals(w.Id, id)));
        }
        public Task<Worker[]> GetWorkersAsync()
        {
            return Task.FromResult(_workers.AsEnumerable().ToArray());
        }
        
    }

    public interface IWorkerService
    {
        Worker GetWorkerById(string id);
        Task<Worker> GetWorkerByIdAsync(string id);
        Task<Worker[]> GetWorkersAsync();
    }
}
