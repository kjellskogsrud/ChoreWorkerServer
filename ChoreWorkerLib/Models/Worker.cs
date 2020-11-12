// <copyright file="Worker.cs" company="Kjell Skogsrud">
// Copyright (c) Kjell Skogsrud. BSD 3-Clause License
// </copyright>

namespace ChoreWorkerLib.Models
{
    /// <summary>
    /// Represents a person doing work.
    /// </summary>
    public class Worker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Worker"/> class.
        /// </summary>
        /// <param name="id">The GUID for this Worker.</param>
        /// <param name="name">The Name for this Worker.</param>
        public Worker(string id, string name) =>
            (this.Id, this.Name) = (id, name);

        /// <summary>
        /// Gets the GUID for this worker.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets or Sets the name of this worker.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the monthly value for this workers chores.
        /// </summary>
        public decimal MonthlyChoreValue { get; set; }
    }
}
