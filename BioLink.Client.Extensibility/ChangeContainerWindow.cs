/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using BioLink.Data;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {

    public class ChangeContainerWindow : Window, IChangeContainer {

        private ChangeContainerImpl _impl;
        private User _user;

        #region designer ctor
        public ChangeContainerWindow() {
        }
        #endregion

        public ChangeContainerWindow(User user) {
            this.User = user;
            _impl = new ChangeContainerImpl(user);
            _impl.ChangeRegistered += new PendingChangedRegisteredHandler(_impl_ChangeRegistered);
            _impl.ChangesCommitted += new PendingChangesCommittedHandler(_impl_ChangesCommitted);
        }

        void _impl_ChangesCommitted(object sender) {
            if (this.ChangesCommitted != null) {
                ChangesCommitted(sender);
            }
        }

        void _impl_ChangeRegistered(object sender, object command) {
            if (this.ChangeRegistered != null) {
                ChangeRegistered(sender, command);
            }
        }

        public bool HasPendingChanges {
            get { return _impl.HasPendingChanges; }
        }

        public void RegisterPendingChange(DatabaseCommand command, object contributer) {
            _impl.RegisterPendingChange(command, contributer);
        }

        public bool RegisterUniquePendingChange(DatabaseCommand command, object contributer) {
            return _impl.RegisterUniquePendingChange(command, contributer);
        }

        public void RegisterPendingChanges(List<DatabaseCommand> commands, object contributer) {
            _impl.RegisterPendingChanges(commands, contributer);
        }

        public void ClearPendingChanges() {
            _impl.ClearPendingChanges();
        }

        public void ClearMatchingPendingChanges(Predicate<DatabaseCommand> predicate) {
            _impl.ClearMatchingPendingChanges(predicate);
        }


        public void CommitPendingChanges(Action successAction = null) {
            _impl.CommitPendingChanges(successAction);
        }

        public ObservableCollection<DatabaseCommand> PendingChanges {
            get { return _impl.PendingChanges; }
        }

        public User User {
            get { return _user; }
            protected set {
                _user = value;
                _impl = new ChangeContainerImpl(value);
                _impl.ChangeRegistered += new PendingChangedRegisteredHandler(_impl_ChangeRegistered);
                _impl.ChangesCommitted += new PendingChangesCommittedHandler(_impl_ChangesCommitted);
            }
        }

        public event PendingChangesCommittedHandler ChangesCommitted;

        public event PendingChangedRegisteredHandler ChangeRegistered;

    }

    public class ChangeContainerImpl : IChangeContainer {

        private ObservableCollection<DatabaseCommand> _pendingChanges = new ObservableCollection<DatabaseCommand>();

        private List<IChangeContainerObserver> _observers = new List<IChangeContainerObserver>();

        public ChangeContainerImpl(User user) {
            this.User = user;
        }

        public bool HasPendingChanges {
            get { return _pendingChanges != null && _pendingChanges.Count > 0; }
        }

        public void RegisterPendingChange(DatabaseCommand command, object contributer) {
            if (command != null) {

                command.CheckPermissions(User);

                _pendingChanges.Add(command);
                if (contributer is IChangeContainerObserver && !_observers.Contains(contributer)) {
                    _observers.Add(contributer as IChangeContainerObserver);
                }
                if (ChangeRegistered != null) {
                    ChangeRegistered(this, command);
                }
            }
        }

        public bool RegisterUniquePendingChange(DatabaseCommand command, object contributer) {
            foreach (DatabaseCommand existingcommand in _pendingChanges) {
                if (existingcommand.Equals(command)) {
                    return false;
                }
            }
            RegisterPendingChange(command, contributer);
            return true;
        }

        public void RegisterPendingChanges(List<DatabaseCommand> commands, object contributer) {
            foreach (DatabaseCommand command in commands) {
                RegisterPendingChange(command, contributer);
            }
        }

        public void ClearPendingChanges() {
            _pendingChanges.Clear();
        }

        public void ClearMatchingPendingChanges(Predicate<DatabaseCommand> predicate) {
            var purgeList = new List<DatabaseCommand>();
            // Build a list of the database commands that need to be removed...
            _pendingChanges.ForEach(command => {
                if (predicate(command)) {
                    purgeList.Add(command);
                }
            });

            // and remove them
            purgeList.ForEach(command => {
                _pendingChanges.Remove(command);
            });
        }


        public void CommitPendingChanges(Action successAction = null) {

            if (User == null) {
                throw new Exception("User object has not been set on the Control Host Window");
            }
#if DEBUG
            Logger.Debug("About to commit the following changes:");
            foreach (DatabaseCommand command in _pendingChanges) {
                Logger.Debug("{0}", command);
            }
#endif


            // First validate each command...Commands can produce messages if they are not valid.
            var messageList = new List<string>();
            foreach (DatabaseCommand command in _pendingChanges) {
                var messages = command.Validate();
                if (messages != null && messages.Count > 0) {
                    messageList.AddRange(messages);
                }
            }

            if (messageList.Count > 0) {
                ErrorMessage.Show("One or more validation errors occured:\n\n{0}\n\nOperation aborted.", messageList.Join("\n\n"));
                return;
            }

            // It may be that this control is aggregated as part of a larger control. This means that, come save time, there
            // may already be a transaction pending. If so, don't create a new one, just piggy back on the existing
            bool commitTrans = false;  // flag to let us know if we are responsible for the transaction...

            if (!User.InTransaction) {
                User.BeginTransaction();
                commitTrans = true;
            }
            try {
                foreach (DatabaseCommand command in _pendingChanges) {
                    command.Process(User);
                }

                if (commitTrans) {
                    User.CommitTransaction();
                }

                if (successAction != null) {
                    successAction();
                }

                foreach (IChangeContainerObserver observer in _observers) {
                    observer.OnChangesCommitted();
                }

                if (ChangesCommitted != null) {
                    ChangesCommitted(this);
                }

                _observers.Clear();
                _pendingChanges.Clear();

                // Reload the pinboard...
                JobExecutor.QueueJob(() => {
                    PluginManager.Instance.RefreshPinBoard();
                });

            } catch (Exception ex) {
                if (commitTrans) {
                    User.RollbackTransaction();
                }
                GlobalExceptionHandler.Handle(ex);
            }
        }

        public ObservableCollection<DatabaseCommand> PendingChanges {
            get { return _pendingChanges; }
        }

        public User User { get; protected set; }

        public event PendingChangesCommittedHandler ChangesCommitted;

        public event PendingChangedRegisteredHandler ChangeRegistered;
    }
}
