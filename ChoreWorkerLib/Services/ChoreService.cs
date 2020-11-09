using ChoreWorkerLib.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using System.Reflection.PortableExecutable;

namespace ChoreWorkerLib.Services
{
    public class ChoreService
    {
        private IList<Chore> _chores;
        
        private bool IsSortedAscending;
        private string CurrentSortField;

        public ChoreService()
        {
            _chores = new List<Chore>();
            DeserializeChores();
            SortChores("Date");
        }
        
        public void SortChores(string field)
        {
            if(field != CurrentSortField)
            {
                SortChores(field, true);
            }
            else
            {
                SortChores(field, !IsSortedAscending);
            }
        }
        public void SortChores(string field,bool asc)
        {
            CurrentSortField = field;
            if(!asc)
            {
                _chores = _chores.OrderByDescending(x => x.GetType().GetProperty(field).GetValue(x, null)).ToList();
                IsSortedAscending = false;
            }
            else
            {
                _chores = _chores.OrderBy(x => x.GetType().GetProperty(field).GetValue(x, null)).ToList();
                IsSortedAscending = true;
            }
            CurrentSortField = field;
        }
        public void SerializeChores()
        {
            XmlSerializer xmlSerial = new XmlSerializer(typeof(List<Chore>));
            FileStream ChoresXML = new FileStream("chores.xml", FileMode.Create);
            xmlSerial.Serialize(ChoresXML, _chores);
            ChoresXML.Close();
        }
        public void DeserializeChores()
        {
            XmlSerializer xmlSerial = new XmlSerializer(typeof(List<Chore>));
            FileStream choresXML = new FileStream("chores.xml", FileMode.Open);
            _chores = (List<Chore>)xmlSerial.Deserialize(choresXML);
            choresXML.Close();
            foreach (Chore c in _chores)
            {
                c.SubscribeForChange(OnChoreChange);
            }
        }
        public Chore GetChoreById(string id)
        {
            return GetChoreByIdAsync(id).Result;
        }
        public Task<Chore> GetChoreByIdAsync(string id)
        {
            return Task.FromResult(_chores.Single(w => Equals(w.Id, id)));
        }
        public Task<Chore[]> GetChoresAsync()
        {
            return Task.FromResult(_chores.AsEnumerable().ToArray());
        }
        public Task<Chore[]> GetChoresForWorkerAsync(string id)
        {
            return Task.FromResult(_chores.Where(c => c.Worker == id).Select(c => c).AsEnumerable().ToArray());
        }
        public Task<Chore[]> GetChoresForSameWeek(string workerId, DateTime day)
        {
            List<Chore> workerChores = _chores.Where(c => c.Worker == workerId).Select(c => c).ToList();
            return Task.FromResult(workerChores.Where(c => DatesAreSameWeek(day,c.Date)).Select(c => c).AsEnumerable().ToArray());

        }
        public Task<Chore[]> GetChoresForMonth(string workerId, DateTime month)
        {
            List<Chore> workerChores = _chores.Where(c => c.Worker == workerId).Select(c => c).ToList();
            return Task.FromResult(workerChores.Where(c => DatesAreSameMonth(c.Date, month)).Select(c => c).AsEnumerable().ToArray());
        }
        public void MakeChores(DateTime fromDate, DateTime toDate, string name, string desctiption, string worker)
        {
            DateTime now = fromDate;
            while(now.Date <= toDate.Date)
            {
                Chore newChore = new Chore(Guid.NewGuid().ToString(),name,desctiption,now);
                newChore.SetWorkerById(worker);
                newChore.SubscribeForChange(OnChoreChange);
                _chores.Add(newChore);
                now = now.AddDays(1);
                Console.WriteLine(newChore.Date);
            }
            Console.WriteLine("DONE");
        }
        public Dictionary<DateTime,int> GetMondaysAndWeeks()
        {
            SortChores("Date",false);
            Dictionary<DateTime, int> result = new Dictionary<DateTime, int>();
            Calendar cal = DateTimeFormatInfo.CurrentInfo.Calendar;
            foreach (Chore c in _chores)
            {
                // Get the week from a data
                int weekNum = cal.GetWeekOfYear(c.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                // Find the Monday in that week
                DateTime Monday = c.Date.AddDays(-(int)c.Date.DayOfWeek+1);
                if (!result.ContainsKey(Monday))
                    result.Add(Monday, weekNum);
            }
            return result;
        }
        public DateTime[] GetMonths()
        {
            List<DateTime> _results = new List<DateTime>();
            // save current sort?
            string currentField = this.CurrentSortField;
            bool currentAsc = IsSortedAscending;

            //Sort by date desc
            SortChores("Date", false);

            foreach (Chore c in _chores)
            {
                if (!_results.Contains(c.Date.AddDays(1 - c.Date.Day)))
                    _results.Add(c.Date.AddDays(1 - c.Date.Day));
            }
            // fix the sort
            SortChores(currentField, currentAsc);
            return _results.ToArray();
        }
        public void CycleState(Chore chore)
        {
            chore.CycleState();
        }
        void OnChoreChange(Chore chore)
        {
            SerializeChores();
        }
        private bool DatesAreSameWeek(DateTime date1, DateTime date2)
        {
            Calendar cal = DateTimeFormatInfo.CurrentInfo.Calendar;
            if(cal.GetWeekOfYear(date1,CalendarWeekRule.FirstFourDayWeek,DayOfWeek.Monday) == cal.GetWeekOfYear(date2, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday))
            {
                return true;
            }
            return false;
        }
        private bool DatesAreSameMonth(DateTime date1, DateTime date2)
        {
            if(date1.Year == date2.Year && date1.Month == date2.Month)
            {
                return true;
            }
            return false;
        }
    }

    public interface IChoreService
    {
        Chore GetChoreById(string id);
        Task<Chore> GetChoreByIdAsync(string id);
        Task<Chore[]> GetChoresAsync();
    }
}
