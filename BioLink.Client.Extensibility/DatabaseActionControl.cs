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

            Service.BeginTransaction();
            try {
                foreach (DatabaseAction<T> action in _pendingChanges) {
                    action.Process(Service);
                }
                Service.CommitTransaction();

                if (successAction != null) {
                    successAction();
                }

                if (PendingChangesCommitted != null) {
                    PendingChangesCommitted(this);
                }

                _pendingChanges.Clear();
            } catch (Exception ex) {
                Service.RollbackTransaction();
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
