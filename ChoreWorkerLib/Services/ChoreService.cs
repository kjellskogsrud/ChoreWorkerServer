// <copyright file="ChoreService.cs" company="Kjell Skogsrud">
// Copyright (c) Kjell Skogsrud. BSD 3-Clause License
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChoreWorkerLib.Models;
using Newtonsoft.Json;

namespace ChoreWorkerLib.Services
{
    /// <summary>
    /// Singleton Service that manages the chores.
    /// </summary>
    public class ChoreService
    {
        private IList<Chore> chores;
        private bool isSortedAscending;
        private string currentSortField = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoreService"/> class.
        /// </summary>
        public ChoreService()
        {
            this.chores = new List<Chore>();
            this.DeserializeChores();

            foreach (Chore c in this.chores)
            {
                c.SubscribeForChange(this.OnChoreChange);
            }

            this.SortChores("Date");
        }

        /// <summary>
        /// Sort chores by the field. Sorts ascending first and decending on a second call with the same field.
        /// </summary>
        /// <param name="field">A string corresponding to a field in the chore to sort by.</param>
        public void SortChores(string field)
        {
            if (field != this.currentSortField)
            {
                this.SortChores(field, true);
            }
            else
            {
                this.SortChores(field, !this.isSortedAscending);
            }
        }

        /// <summary>
        /// Sort chores by the field.
        /// </summary>
        /// <param name="field">A string corresponding to a field in the chore to sort by.</param>
        /// <param name="asc">Sort ascending if true, decending if false.</param>
        public void SortChores(string field, bool asc)
        {
            this.currentSortField = field;
            if (!asc)
            {
                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                this.chores = this.chores.OrderByDescending(x => x.GetType().GetProperty(field).GetValue(x, null)).ToList();
                this.isSortedAscending = false;
                #pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            else
            {
                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                this.chores = this.chores.OrderBy(x => x.GetType().GetProperty(field).GetValue(x, null)).ToList();
                this.isSortedAscending = true;
                #pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
        }

        /// <summary>
        /// Serialize the list of chores to chores.json.
        /// </summary>
        public void SerializeChores()
        {
            StreamWriter choresJson = new StreamWriter("chores.json");
            choresJson.Write(JsonConvert.SerializeObject(this.chores, Formatting.Indented));
            choresJson.Close();
        }

        /// <summary>
        /// Deserialize the list of chores from chores.json.
        /// </summary>
        public void DeserializeChores()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };

            StreamReader choresJson = new StreamReader("chores.json");
            this.chores = JsonConvert.DeserializeObject<List<Chore>>(choresJson.ReadToEnd(),settings);
            choresJson.Close();
        }

        /// <summary>
        /// Gets a chore by its GUID.
        /// </summary>
        /// <param name="id">GUID of the chore.</param>
        /// <returns><see cref="Chore"/>.</returns>
        public Chore GetChoreById(string id)
        {
            return this.GetChoreByIdAsync(id).Result;
        }

        /// <summary>
        /// Gets a chore by its GUID, Async.
        /// </summary>
        /// <param name="id">GUID of the chore.</param>
        /// <returns><see cref="Chore"/>.</returns>
        public Task<Chore> GetChoreByIdAsync(string id)
        {
            return Task.FromResult(this.chores.Single(w => Equals(w.Id, id)));
        }

        /// <summary>
        /// Gets all the chores. Async.
        /// </summary>
        /// <returns>Array off <see cref="Chore"/>.</returns>
        public Task<Chore[]> GetChoresAsync()
        {
            return Task.FromResult(this.chores.AsEnumerable().ToArray());
        }

        /// <summary>
        /// Gets all the chores for a worker.
        /// </summary>
        /// <param name="id">GUID of the worker.</param>
        /// <returns>Array off <see cref="Chore"/>.</returns>
        public Task<Chore[]> GetChoresForWorkerAsync(string id)
        {
            return Task.FromResult(this.chores.Where(c => c.Worker == id).Select(c => c).AsEnumerable().ToArray());
        }

        /// <summary>
        /// Gets all the chores for a worker that are in the same week as the specified date.
        /// Weeks start on Monday.
        /// </summary>
        /// <param name="workerId">GUID of the worker.</param>
        /// <param name="date">A date.</param>
        /// <returns>Array off <see cref="Chore"/>.</returns>
        public Task<Chore[]> GetChoresForSameWeek(string workerId, DateTime date)
        {
            List<Chore> workerChores = this.chores.Where(c => c.Worker == workerId).Select(c => c).ToList();
            return Task.FromResult(workerChores.Where(c => this.DatesAreSameWeek(date, c.Date)).Select(c => c).AsEnumerable().ToArray());
        }

        /// <summary>
        /// Gets all the chores for a worker that are in the same month as the specified date.
        /// </summary>
        /// <param name="workerId">GUID of the worker.</param>
        /// <param name="date">A date.</param>
        /// <returns>Array off <see cref="Chore"/>.</returns>
        public Task<Chore[]> GetChoresForMonth(string workerId, DateTime date)
        {
            List<Chore> workerChores = this.chores.Where(c => c.Worker == workerId).Select(c => c).ToList();
            return Task.FromResult(workerChores.Where(c => this.DatesAreSameMonth(c.Date, date)).Select(c => c).AsEnumerable().ToArray());
        }

        /// <summary>
        /// Makes Chores.
        /// </summary>
        /// <param name="fromDate">First date for this chore.</param>
        /// <param name="toDate">Last date for this chore.</param>
        /// <param name="name">The name of this chore.</param>
        /// <param name="desctiption">The description of this chore.</param>
        /// <param name="worker">The GUID of the worker assigned to this chore.</param>
        public void MakeChores(DateTime fromDate, DateTime toDate, string name, string desctiption, string worker)
        {
            DateTime now = fromDate;
            while (now.Date <= toDate.Date)
            {
                Chore newChore = new Chore(Guid.NewGuid().ToString(), name, desctiption, now);
                newChore.SetWorkerById(worker);
                newChore.SubscribeForChange(this.OnChoreChange);
                this.chores.Add(newChore);
                now = now.AddDays(1);
                Console.WriteLine(newChore.Date);
            }

            Console.WriteLine("DONE");
        }

        /// <summary>
        /// Gets all the weeks that have chores.
        /// Weeks start on Monday, and the first week of a year is the first week with a majority (4 or more) of its days in January.
        /// </summary>
        /// <returns>A Dictionary of DateTime Mondays, and their Week number.</returns>
        public Dictionary<DateTime, int> GetMondaysAndWeeks()
        {
            this.SortChores("Date", false);
            Dictionary<DateTime, int> result = new Dictionary<DateTime, int>();
            Calendar cal = DateTimeFormatInfo.CurrentInfo.Calendar;
            foreach (Chore c in this.chores)
            {
                // Get the week from a data
                int weekNum = cal.GetWeekOfYear(c.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                // Find the Monday in that week
                DateTime monday = c.Date.AddDays(-(int)c.Date.DayOfWeek + 1);
                if (!result.ContainsKey(monday))
                {
                    result.Add(monday, weekNum);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all months that have chores.
        /// </summary>
        /// <returns>An Array of <see cref="DateTime"/>. The first of each month.</returns>
        public DateTime[] GetMonths()
        {
            List<DateTime> results = new List<DateTime>();

            // Save current sort?
            string currentField = this.currentSortField;
            bool currentAsc = this.isSortedAscending;

            // Sort by date desc
            this.SortChores("Date", false);

            foreach (Chore c in this.chores)
            {
                if (!results.Contains(c.Date.AddDays(1 - c.Date.Day)))
                {
                    results.Add(c.Date.AddDays(1 - c.Date.Day));
                }
            }

            // Fix the sort
            this.SortChores(currentField, currentAsc);
            return results.ToArray();
        }

        private void OnChoreChange(Chore chore)
        {
            this.SerializeChores();
        }

        private bool DatesAreSameWeek(DateTime date1, DateTime date2)
        {
            Calendar cal = DateTimeFormatInfo.CurrentInfo.Calendar;
            if (cal.GetWeekOfYear(date1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) == cal.GetWeekOfYear(date2, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday))
            {
                return true;
            }

            return false;
        }

        private bool DatesAreSameMonth(DateTime date1, DateTime date2)
        {
            if (date1.Year == date2.Year && date1.Month == date2.Month)
            {
                return true;
            }

            return false;
        }
    }
}
