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
            XmlSerializer xmlSerial = new XmlSerializer(typeof(List<Worker>));
            FileStream workersXML = new FileStream("workers.xml", FileMode.Create);
            xmlSerial.Serialize(workersXML, _workers);
            workersXML.Close();
        }
        public void DeserializeWorkers()
        {
            XmlSerializer xmlSerial = new XmlSerializer(typeof(List<Worker>));
            FileStream workersXML = new FileStream("workers.xml", FileMode.Open);
            _workers = (List<Worker>)xmlSerial.Deserialize(workersXML);
            workersXML.Close();
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
