using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BioLink.Data;
using BioLink.Client.Utilities;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BioLink.Client.Extensibility {

    public class DatabaseCommandControl : UserControl, IIdentifiableContent, IChangeContainerObserver, IDisposable, IIconHolder {

        #region Designer Constructor
        public DatabaseCommandControl() : base() {
        }
        #endregion

        public DatabaseCommandControl(User user, string contentId)
            : base() {

            this.User = user;
            this.ContentIdentifier = contentId;
        }

        public string ContentIdentifier { get; private set; }

        public bool CompareContentHash(string other) {
            return ContentIdentifier == other;
        }

        public void RegisterPendingChange(DatabaseCommand command) {
            WithChangeContainer(window => {
                window.RegisterPendingChange(command, this);
            });
            RaiseChangeRegistered(command);
        }

        public bool RegisterUniquePendingChange(DatabaseCommand command) {
            bool ret = false;
            WithChangeContainer(window => { ret = window.RegisterUniquePendingChange(command, this); });
            RaiseChangeRegistered(command);
            return ret;
        }

        public void RegisterPendingChanges(List<DatabaseCommand> commands) {
            WithChangeContainer(window =>  window.RegisterPendingChanges(commands, this));
            RaiseChangeRegistered(commands);

        }

        protected void CommitPendingChanges(Action successAction = null) {
            WithChangeContainer(window => window.CommitPendingChanges(successAction));            
        }

        public void ClearPendingChanges() {
            WithChangeContainer(window => window.ClearPendingChanges());
        }

        public void ClearMatchingPendingChanges(Predicate<DatabaseCommand> predicate) {
            WithChangeContainer(container => {
                container.ClearMatchingPendingChanges(predicate);
            });
        }

        public virtual bool Validate(List<String> messages) {
            return true;
        }

        protected string WindowTitle {
            get {
                var window = this.FindParentWindow();
                if (window != null) {
                    return window.Title;
                }
                return "";
            }

            set {
                var window = this.FindParentWindow();
                if (window != null) {
                    window.Title = value;
                }
            }
        }

        private IChangeContainer FindChangeContainer() {
            var p = this as FrameworkElement;

            while (!(p is IChangeContainer) && p != null) {
                p = p.Parent as FrameworkElement;
            }

            if (p != null) {
                return (IChangeContainer) p;
            }
            return null;
        }

        private bool WithChangeContainer(Action<IChangeContainer> action) {
            var container = FindChangeContainer();
            if (container != null) {
                action(container);
                return true;
            } else {
                return false;
            }
        }

        public bool HasPendingChanges {
            get {
                bool result = false;
                WithChangeContainer((container) => {
                    result = container.HasPendingChanges;
                });
                return result;
            }
        }

        public void OnChangesCommitted() {
            if (ChangesCommitted != null) {
                ChangesCommitted(this);
            }
        }

        protected string NextNewName<T>(string format, IEnumerable<T> collection, Expression<Func<string>> nameProperty, string pattern = @"(\d+)") {
            var destProp = (PropertyInfo)((MemberExpression)nameProperty.Body).Member;
            Regex regex = new Regex("^" + string.Format(format, pattern) + "$");
            int nextNum = 1;
            foreach (T t in collection) {
                string name = (string)destProp.GetValue(t, null);
                var m = regex.Match(name);
                if (m.Success) {
                    int candidate;
                    if (Int32.TryParse(m.Groups[1].Value, out candidate)) {
                        if (candidate >= nextNum) {
                            nextNum = candidate + 1;
                        }
                    }
                }
            }
            return string.Format(format, nextNum);
        }

        private void RaiseChangeRegistered(DatabaseCommand change) {
            var list = new List<DatabaseCommand>();
            list.Add(change);
            RaiseChangeRegistered(list);
        }

        private void RaiseChangeRegistered(List<DatabaseCommand> list) {
            if (ChangeRegistered != null) {
                ChangeRegistered(list);
            }
        }

        public virtual void Dispose() {        
        }

        public User User { get; protected set; }

        public event Action<IList<DatabaseCommand>> ChangeRegistered;

        public event PendingChangesCommittedHandler ChangesCommitted;

        public event Action ChangeContainerSet;


        public virtual System.Windows.Media.ImageSource Icon {
            get { return null; }
        }

        public void NotifyChangeContainerSet() {
            if (ChangeContainerSet != null) {
                ChangeContainerSet();
            }
        }

        public virtual void RefreshContent() {
        }

    }

    public interface IIdentifiableContent {
        string ContentIdentifier { get; }
        void RefreshContent();
    }
    
}
