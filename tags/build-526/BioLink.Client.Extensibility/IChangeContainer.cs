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
using BioLink.Data;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {

    public interface IChangeContainer {

        void RegisterPendingChange(DatabaseCommand change, object contributer);

        bool RegisterUniquePendingChange(DatabaseCommand change, object contributer);

        void RegisterPendingChanges(List<DatabaseCommand> commands, object contributer);

        void CommitPendingChanges(Action successAction = null);

        void ClearPendingChanges();

        void ClearMatchingPendingChanges(Predicate<DatabaseCommand> predicate);

        bool HasPendingChanges { get; }

        ObservableCollection<DatabaseCommand> PendingChanges { get; }
    }

    public interface IChangeContainerObserver {

        void OnChangesCommitted();

    }
}
