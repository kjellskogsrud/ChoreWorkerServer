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
        private IList<Chore>? chores;
        private bool isSortedAscending;
        private string currentSortField = string.Empty;

        // For saveing the last sort state
        private string lastSortField = string.Empty;
        private bool lastSortAsc;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoreService"/> class.
        /// </summary>
        public ChoreService()
        {
            this.chores = new List<Chore>();
            this.DeserializeChores();
            if (this.chores == null)
            {
                Chore sampleChore = new Chore(Guid.NewGuid().ToString(), "Sample name", "Sample Description", DateTime.Now);
                this.chores = new List<Chore>();
                this.chores.Add(sampleChore);
                this.SerializeChores();
                Environment.Exit(1);
            }

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
            this.chores = JsonConvert.DeserializeObject<List<Chore>>(choresJson.ReadToEnd(), settings);
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
        /// Gets all chores in the given year and month for the given worker.
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="worker">the GUID of a worker.</param>
        /// <returns>A subset of chores where the year, month and worker equels the parameters.</returns>
        public Task<Chore[]> GetChoresAsync(int year, int month, string worker)
        {
            DateTime qMonth = new DateTime(year, month, 1);
            List<Chore> monthChores = this.chores.Where(c => this.DatesAreSameMonth(qMonth, c.Date)).Select(c => c).ToList();
            return Task.FromResult(monthChores.Where(c => c.Worker == worker).Select(c => c).AsEnumerable().ToArray());
        }

        /// <summary>
        /// Gets all chores on the give date, and for the following amount of days.
        /// Ie. Using this with any monday, and 6 days will get you a week of chores.
        /// </summary>
        /// <param name="startDate">The first day to get chores for.</param>
        /// <param name="days">The amount of days more to get.</param>
        /// <param name="worker">the GUID of a worker.</param>
        /// <returns>A subset of chores where the first item is on the startDate.</returns>
        /// <remarks>Does not support sorting. Will always return sorted by Date decending.</remarks>
        public Task<Chore[]> GetChoresAsync(DateTime startDate, int days, string worker)
        {
            DateTime endDate = startDate.AddDays(days);
            this.SortAndSaveLast("Date", true);
            List<Chore> timeChores = this.chores.Where(c => c.Date >= startDate && c.Date <= endDate).Select(c => c).ToList();
            this.RevertSort();
            return Task.FromResult(timeChores.Where(c => c.Worker == worker).Select(c => c).AsEnumerable().ToArray());
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
        /// Makes Chores.
        /// </summary>
        /// <param name="fromDate">First date for this chore.</param>
        /// <param name="toDate">Last date for this chore.</param>
        /// <param name="name">The name of this chore.</param>
        /// <param name="desctiption">The description of this chore.</param>
        /// <param name="worker">The GUID of the worker assigned to this chore.</param>
        /// <returns>The number of chores made.</returns>
        public int MakeChores(DateTime fromDate, DateTime toDate, string name, string desctiption, string worker)
        {
            int counter = 0;
            DateTime now = fromDate;
            while (now.Date <= toDate.Date)
            {
                if (this.chores == null)
                {
                    this.chores = new List<Chore>();
                }

                Chore newChore = new Chore(Guid.NewGuid().ToString(), name, desctiption, now);
                newChore.SetWorkerById(worker);
                newChore.SubscribeForChange(this.OnChoreChange);
                this.chores.Add(newChore);
                now = now.AddDays(1);
                counter++;
            }

            this.SerializeChores();
            return counter;
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
            if (this.chores == null)
            {
                return result;
            }

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
        public Task<DateTime[]> GetMonths()
        {
            List<DateTime> results = new List<DateTime>();
            this.SortAndSaveLast("Date", false);

            // There are no chores so we return the empty list
            if (this.chores == null)
            {
                return Task.FromResult(results.ToArray());
            }

            foreach (Chore chore in this.chores)
            {
                if (!results.Contains(chore.Date.AddDays(1 - chore.Date.Day)))
                {
                    results.Add(chore.Date.AddDays(1 - chore.Date.Day));
                }
            }

            this.RevertSort();
            return Task.FromResult(results.ToArray());
        }

        /// <summary>
        /// Gets all weeks that have chores.
        /// </summary>
        /// <returns>An Array of <see cref="DateTime"/>. The first of each week is a monday.</returns>
        public Task<DateTime[]> GetWeeks()
        {
            List<DateTime> results = new List<DateTime>();

            // Sort by date desc
            this.SortAndSaveLast("Date", false);

            // There are no chores so we return the empty list
            if (this.chores == null)
            {
                return Task.FromResult(results.ToArray());
            }

            foreach (Chore chore in this.chores)
            {
                if (!results.Contains(chore.Date.AddDays(0 - chore.Date.DayOfWeek + 1)))
                {
                    results.Add(chore.Date.AddDays(0 - chore.Date.DayOfWeek + 1));
                }
            }

            this.RevertSort();
            return Task.FromResult(results.ToArray());
        }

        /// <summary>
        /// Delete a chore.
        /// </summary>
        /// <param name="chore">The Chore to delete.</param>
        /// <returns>True on success.</returns>
        public bool DeleteChore(Chore chore)
        {
            if (this.chores != null)
            {
                if (this.chores.Contains(chore) && !chore.Locked)
                {
                    if (this.chores.Remove(chore))
                    {
                        return true;
                    }
                }
            }

            return false;
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

        private void SortAndSaveLast(string field, bool asc)
        {
            // Save current sort
            this.lastSortField = this.currentSortField;
            this.lastSortAsc = this.isSortedAscending;

            // Sort by date desc
            this.SortChores(field, asc);
        }

        private void RevertSort()
        {
            // Fix the sort
            this.SortChores(this.lastSortField, this.lastSortAsc);
        }
    }
}
