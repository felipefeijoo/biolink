using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class DatabaseActionControl : UserControl, IIdentifiableContent, IChangeContainerObserver {

        #region Designer Constructor
        public DatabaseActionControl() : base() {
        }
        #endregion

        public DatabaseActionControl(User user, string contentId)
            : base() {

            this.User = user;
            this.ContentIdentifier = contentId;
        }

        public string ContentIdentifier { get; private set; }

        public bool CompareContentHash(string other) {
            return ContentIdentifier == other;
        }

        public void RegisterPendingChange(DatabaseAction action) {
            WithChangeContainer(window => {
                window.RegisterPendingChange(action, this);
            });
        }

        public bool RegisterUniquePendingChange(DatabaseAction action) {
            bool ret = false;
            WithChangeContainer(window => { ret = window.RegisterUniquePendingChange(action, this); });
            return ret;
        }

        public void RegisterPendingChanges(List<DatabaseAction> actions) {
            WithChangeContainer(window =>  window.RegisterPendingChanges(actions, this));
        }

        protected void CommitPendingChanges(Action successAction = null) {
            WithChangeContainer(window => window.CommitPendingChanges(successAction));            
        }

        public void ClearPendingChanges() {
            WithChangeContainer(window => window.ClearPendingChanges());
        }

        private void WithChangeContainer(Action<IChangeContainer> action) {
            var window = this.FindParentWindow() as IChangeContainer;
            if (window != null) {
                action(window);
            } else {
                throw new Exception("Parent window could not be found, or it is not an IChangeContainer");
            }

        }

        public void OnChangesCommitted() {
            if (ChangesCommitted != null) {
                ChangesCommitted(this);
            }
        }


        public virtual void Dispose() {        
        }

        public User User { get; private set; }

        public event PendingChangesCommittedHandler ChangesCommitted;

    }

    public interface IIdentifiableContent {
        string ContentIdentifier { get; }        
    }
    
}
