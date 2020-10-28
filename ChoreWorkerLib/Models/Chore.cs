using System;
using System.Diagnostics.CodeAnalysis;


namespace ChoreWorkerLib.Models
{
    /// <summary>
    /// Represents a real world job to be done
    /// </summary>
    
    public class Chore
    {
        #region Public members
        public string Id { get; private set; } = "";
        public string Name { get; private set; } = "";
        public string Description { get; private set; } = "";
        /// <summary>
        /// Represents when this chore is due.
        /// </summary>
        public DateTime Date { get; private set; }
        /// <summary>
        /// When the chore was last modified.
        /// </summary>
        public DateTime LastModified { get; private set; }
        /// <summary>
        /// When the chore was first marked as completed(or paused).
        /// </summary>
        public DateTime? Completed { get; private set; } = null;
        /// <summary>
        /// The person set to do the chore.
        /// </summary>
        public Worker? Worker { get; private set; }
        /// <summary>
        /// State of this chore.
        /// </summary>
        public ChoreState State { get; private set; } = ChoreState.BLANK;

        #endregion
        #region Enumerations
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
            NOTDONE = 16

        }
        #endregion
        #region Constructors
        public Chore(string id, string name, string description, DateTime date) =>
            (this.Id, this.Name, this.Description, this.Date, this.LastModified) = (id, name, description, date, DateTime.Now);
        #endregion
        #region Public Methods
        public bool SetWorker(Worker worker)
        {
            Worker = worker;
            SetLastModifiedNow();
            return true;
        }
        public bool SetState(ChoreState state)
        {
            if(state == ChoreState.DONE || state == ChoreState.PAUSED)
            {
                if(this.Completed == null)
                {
                    this.Completed = DateTime.Now;
                }
            }
            this.State = state;
            SetLastModifiedNow();
            return true;
        }
        #endregion
        #region Private Methods
        private void SetLastModifiedNow()
        {
            this.LastModified = DateTime.Now;
        }
        #endregion

    }
}
