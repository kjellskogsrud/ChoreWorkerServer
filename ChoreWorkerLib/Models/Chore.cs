// <copyright file="Chore.cs" company="Kjell Skogsrud">
// Copyright (c) Kjell Skogsrud. BSD 3-Clause License
// </copyright>

using System;
using Newtonsoft.Json;

namespace ChoreWorkerLib.Models
{
    /// <summary>
    /// Represents a real world job to be done.
    /// </summary>
    public class Chore
    {
        private Action<Chore>? cbChoreChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Chore"/> class.
        /// </summary>
        /// <param name="id">The GUID for this chore.</param>
        /// <param name="name">The name for this chore.</param>
        /// <param name="description">The Description for this chore.</param>
        /// <param name="date">The date for this chore.</param>
        [JsonConstructor]
        public Chore(string id, string name, string description, DateTime date) =>
            (this.Id, this.Name, this.Description, this.Date, this.LastModified) = (id, name, description, date, DateTime.Now);

        /// <summary>
        /// Represent the state of the chore.
        /// </summary>
        public enum ChoreState
        {
            /// <summary>
            /// An blank chore.
            /// Indicates that it has not yet been done. Think of blank as a checkbox that has not yet been ticked.
            /// </summary>
            BLANK = 2,

            /// <summary>
            /// A done chore.
            /// Indicated that it has been completed successfully.
            /// </summary>
            DONE = 4,

            /// <summary>
            /// A chore that has not been done, but counts as complete anyway.
            /// Typically something that only needs to be done some days and then just checked wether it needs to be done again.
            /// Paused is for when it is not possible for the worker to complete the chore, but it was done right before in a best
            /// effort to keep it done while the worker is unavaialble.
            /// </summary>
            PAUSED = 8,

            /// <summary>
            /// A chore that has not been done. It is now to late to do it.
            /// For when the worker neglects the chore.
            /// </summary>
            NOTDONE = 16,
        }

        /// <summary>
        /// Gets the GUID of the chore.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets or sets the name of the chore.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the chore.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the comment of the chore.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// Gets or sets the date of the chore.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets the last modified datetime of the chore.
        /// </summary>
        public DateTime LastModified { get; private set; }

        /// <summary>
        /// Gets the complete datetime of the chore.
        /// </summary>
        public DateTime? Completed { get; private set; }

        /// <summary>
        /// Gets the worker GUID assigned to this chore.
        /// </summary>
        public string? Worker { get; private set; }

        /// <summary>
        /// Gets the state of the chore.
        /// </summary>
        public ChoreState State { get; private set; } = ChoreState.BLANK;

        /// <summary>
        /// Gets or sets the money value of the chore.
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// Gets a value indicating whether the chore is locked.
        /// </summary>
        public bool Locked { get; private set; }

        /// <summary>
        /// Set the worker GUID for the chore to this workers GUID.
        /// </summary>
        /// <param name="worker">A worker.</param>
        /// <returns>True on success, false on failure.</returns>
        public bool SetWorker(Worker worker) => this.SetWorkerById(worker.Id);

        /// <summary>
        /// Set the worker GUID for the chore to this GUID.
        /// </summary>
        /// <param name="id">A Workers GUID.</param>
        /// <returns>True on success, false on failure.</returns>
        public bool SetWorkerById(string id)
        {
            if (this.Locked)
            {
                return false;
            }

            this.Worker = id;
            this.SetLastModifiedNow();
            return true;
        }

        /// <summary>
        /// Lock the chore. Prevents changes to it while locked.
        /// </summary>
        public void Lock()
        {
            this.Locked = true;
            this.SetLastModifiedNow();
        }

        /// <summary>
        /// Unlock the chore.
        /// </summary>
        public void Unlock()
        {
            this.Locked = false;
            this.SetLastModifiedNow();
        }

        /// <summary>
        /// Sets the state for the chore.
        /// </summary>
        /// <param name="state">State to set.</param>
        /// <returns>True on success, false on failure.</returns>
        public bool SetState(ChoreState state)
        {
            if (this.Locked)
            {
                return false;
            }

            if (state == ChoreState.DONE || state == ChoreState.PAUSED)
            {
                if (this.Completed == null)
                {
                    this.Completed = DateTime.Now;
                }
            }

            this.State = state;
            this.SetLastModifiedNow();
            this.cbChoreChanged?.Invoke(this);

            return true;
        }

        /// <summary>
        /// Cycle the state of the chore.
        /// </summary>
        public void CycleState()
        {
            switch (this.State)
            {
                case ChoreState.BLANK:
                    this.SetState(ChoreState.DONE);
                    break;
                case ChoreState.DONE:
                    this.SetState(ChoreState.PAUSED);
                    break;
                case ChoreState.PAUSED:
                    this.SetState(ChoreState.NOTDONE);
                    break;
                case ChoreState.NOTDONE:
                    this.SetState(ChoreState.BLANK);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Subscribe for changes to the chore.
        /// </summary>
        /// <param name="callback">Method that should be called on change.</param>
        public void SubscribeForChange(Action<Chore> callback)
        {
            this.cbChoreChanged += callback;
        }

        /// <summary>
        /// Unsubscribe for changes to the chore.
        /// </summary>
        /// <param name="callback">Method that should be called on change.</param>
        public void UnsubscribeForChange(Action<Chore> callback)
        {
            this.cbChoreChanged -= callback;
        }

        private void SetLastModifiedNow()
        {
            this.LastModified = DateTime.Now;
        }
    }
}
