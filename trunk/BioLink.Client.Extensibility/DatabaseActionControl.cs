using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class DatabaseActionControl<T> : UserControl, IClosable where T : BioLinkService {

        private List<DatabaseAction<T>> _pendingChanges = new List<DatabaseAction<T>>();

        #region Designer Constructor
        public DatabaseActionControl() : base() {
        }
        #endregion

        public DatabaseActionControl(T service)
            : base() {

            this.Service = service;
        }

        public bool HasPendingChanges {
            get { return _pendingChanges != null && _pendingChanges.Count > 0; }
        }

        protected void RegisterPendingAction<K>(K action) where K : DatabaseAction<T> {
            _pendingChanges.Add((K) action);
            if (PendingChangedRegistered != null) {
                PendingChangedRegistered(this, action);
            }
        }

        protected bool RegisterUniquePendingAction<K>(K action) where K : DatabaseAction<T> {
            foreach (DatabaseAction<T> existingaction in _pendingChanges) {
                if (existingaction.Equals(action)) {
                    return false;
                }
            }
            RegisterPendingAction(action);
            return true;
        }

        protected void RegisterPendingActions<K>(List<K> actions) where K : DatabaseAction<T> {
            foreach (DatabaseAction<T> action in actions) {
                RegisterPendingAction(action);
            }
        }

        protected void ClearPendingChanges() {
            _pendingChanges.Clear();
        }

        protected void CommitPendingChanges(Action successAction) {

            if (Service == null) {
                throw new Exception("Service has not been set onf Database Action Control");
            }
#if DEBUG
            Logger.Debug("About to commit the following changes:");
            foreach (DatabaseAction<T> action in _pendingChanges) {
                Logger.Debug("{0}", action);
            }
#endif

            // It may be that this control is aggregated as part of a larger control. This means that, come save time, there
            // may already be a transaction pending. If so, don't create a new one, just piggy back on the existing
            bool commitTrans = false;  // flag to let us know if we are responsible for the transaction...
            if (!Service.InTransaction) {
                Service.BeginTransaction();
                commitTrans = true;
            }
            try {
                foreach (DatabaseAction<T> action in _pendingChanges) {
                    action.Process(Service);
                }

                if (commitTrans) {
                    Service.CommitTransaction();
                }

                if (successAction != null) {
                    successAction();
                }

                if (PendingChangesCommitted != null) {
                    PendingChangesCommitted(this);
                }

                _pendingChanges.Clear();
            } catch (Exception ex) {
                if (commitTrans) {
                    Service.RollbackTransaction();
                }
                GlobalExceptionHandler.Handle(ex);
            }
        }

        public List<DatabaseAction<T>> PendingChanges {
            get { return _pendingChanges; }
        }

        public bool RequestClose() {
            if (HasPendingChanges) {
                if (this.Question("You have unsaved changes. Are you sure you want to discard those changes?", "Discard changes?")) {
                    return true;
                } else {
                    return false;
                }
            }
            return true;
        }

        public void Dispose() {        
        }

        public T Service { get; private set; }

        public void ApplyChanges() {
            CommitPendingChanges(null);
        }

        public event PendingChangedRegisteredHandler PendingChangedRegistered;

        public event PendingChangesCommittedHandler PendingChangesCommitted;
    }

    
}
