// <copyright file="WorkerService.cs" company="Kjell Skogsrud">
// Copyright (c) Kjell Skogsrud. BSD 3-Clause License
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChoreWorkerLib.Models;
using Newtonsoft.Json;

namespace ChoreWorkerLib.Services
{
    /// <summary>
    /// Singleton Service that manages workers.
    /// </summary>
    public class WorkerService
    {
        private IList<Worker> workers;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkerService"/> class.
        /// </summary>
        public WorkerService()
        {
            this.workers = new List<Worker>();
            this.DeserializeWorkers();
        }

        /// <summary>
        /// Serialize the list of workers to workers.json.
        /// </summary>
        public void SerializeWorkers()
        {
            StreamWriter workersJson = new StreamWriter("workers.json");
            workersJson.Write(JsonConvert.SerializeObject(this.workers, Formatting.Indented));
            workersJson.Close();
        }

        /// <summary>
        /// Serialize the list of workers from workers.json.
        /// </summary>
        public void DeserializeWorkers()
        {
            StreamReader workerJson = new StreamReader("workers.json");
            this.workers = JsonConvert.DeserializeObject<List<Worker>>(workerJson.ReadToEnd());
            workerJson.Close();
        }

        /// <summary>
        /// Gets a worker by its GUID.
        /// </summary>
        /// <param name="id">Worker GUID.</param>
        /// <returns><see cref="Worker"/>.</returns>
        public Worker GetWorkerById(string id)
        {
            return this.GetWorkerByIdAsync(id).Result;
        }

        /// <summary>
        /// Gets a worker by its GUID. Async.
        /// </summary>
        /// <param name="id">Worker GUID.</param>
        /// <returns><see cref="Worker"/>.</returns>
        public Task<Worker> GetWorkerByIdAsync(string id)
        {
            return Task.FromResult(this.workers.Single(w => Equals(w.Id, id)));
        }

        /// <summary>
        /// Gets all the workers. Async.
        /// </summary>
        /// <returns>An array of <see cref="Worker"/>.</returns>
        public Task<Worker[]> GetWorkersAsync()
        {
            return Task.FromResult(this.workers.AsEnumerable().ToArray());
        }
    }
}
