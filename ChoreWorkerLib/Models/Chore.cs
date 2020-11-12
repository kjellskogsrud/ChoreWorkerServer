using Newtonsoft.Json;
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
        public string Id { get;  set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Comment { get; set; } = "";
        public DateTime Date { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime? Completed { get; set; } = null;
        public string? Worker { get;  set; }
        public ChoreState State { get; set; } = ChoreState.BLANK;
        public decimal Value { get; set; }
        public bool Locked { get; set; } = false;
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
        [JsonConstructor]
        public Chore(string id, string name, string description, DateTime date) =>
            (this.Id, this.Name, this.Description, this.Date, this.LastModified) = (id, name, description, date, DateTime.Now);
        #endregion
        #region Public Methods
        public bool SetWorker(Worker worker)
        {
            return SetWorkerById(worker.Id);
            
        }
        public bool SetWorkerById(string id)
        {
            if (this.Locked)
                return false;
            Worker = id;
            SetLastModifiedNow();
            return true;
        }
        public void Lock()
        {
            this.Locked = true;
            SetLastModifiedNow();
        }
        public void Unlock()
        {
            this.Locked = false;
            SetLastModifiedNow();
        }
        public bool SetState(ChoreState state)
        {
            if (this.Locked)
                return false;

            if (state == ChoreState.DONE || state == ChoreState.PAUSED)
            {
                if(this.Completed == null)
                {
                    this.Completed = DateTime.Now;
                }
            }
            this.State = state;
            SetLastModifiedNow();
            if(cbChoreChanged != null)
                cbChoreChanged(this);

            return true;
        }
        public void CycleState()
        {
            switch (this.State)
            {
                case ChoreState.BLANK:
                    SetState(ChoreState.DONE);
                    break;
                case ChoreState.DONE:
                    SetState(ChoreState.PAUSED);
                    break;
                case ChoreState.PAUSED:
                    SetState(ChoreState.NOTDONE);
                    break;
                case ChoreState.NOTDONE:
                    SetState(ChoreState.BLANK);
                    break;
                default:
                    break;
            }
        }

        Action<Chore> cbChoreChanged;

        public void SubscribeForChange(Action<Chore> callback)
        {
            cbChoreChanged += callback;
        }

        public void UnsubscribeForChange(Action<Chore> callback)
        {
            cbChoreChanged -= callback;
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
