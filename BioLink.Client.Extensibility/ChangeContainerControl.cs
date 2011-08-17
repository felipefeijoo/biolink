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
using System.Windows.Controls;
using BioLink.Data;
using BioLink.Client.Utilities;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {

    public class ChangeContainerControl : UserControl, IChangeContainer {

        private ChangeContainerImpl _impl;

        #region designer ctor
        public ChangeContainerControl()
            : base() {
        }
        #endregion

        public ChangeContainerControl(User user)
            : base() {
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

        public bool RequestClose() {
            if (HasPendingChanges) {
                return this.DiscardChangesQuestion();
            }
            return true;
        }

        public User User { get; protected set; }

        public event PendingChangesCommittedHandler ChangesCommitted;

        public event PendingChangedRegisteredHandler ChangeRegistered;

    }
}
