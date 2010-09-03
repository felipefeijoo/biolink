using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class DatabaseActionControl<T> : UserControl where T : BioLinkService {

        private List<DatabaseAction<T>> _pendingChanges = new List<DatabaseAction<T>>();

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

        protected void CommitPendingChanges(T service, Action successAction) {
#if DEBUG
            Logger.Debug("About to commit the following changes:");
            foreach (DatabaseAction<T> action in _pendingChanges) {
                Logger.Debug("{0}", action);
            }
#endif

            service.BeginTransaction();
            try {
                foreach (DatabaseAction<T> action in _pendingChanges) {
                    action.Process(service);
                }
                service.CommitTransaction();

                if (successAction != null) {
                    successAction();
                }

                _pendingChanges.Clear();
            } catch (Exception ex) {
                service.RollbackTransaction();
                GlobalExceptionHandler.Handle(ex);
            }
        }

        public List<DatabaseAction<T>> PendingChanges {
            get { return _pendingChanges; }
        }

        public event PendingChangedRegisteredHandler PendingChangedRegistered;

        public delegate void PendingChangedRegisteredHandler(object sender, DatabaseAction<T> action);

    }

    
}
