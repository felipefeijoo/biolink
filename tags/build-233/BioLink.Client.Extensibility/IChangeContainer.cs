using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace BioLink.Client.Extensibility {

    public interface IChangeContainer {

        void RegisterPendingChange(DatabaseAction change, object contributer);

        bool RegisterUniquePendingChange(DatabaseAction change, object contributer);

        void RegisterPendingChanges(List<DatabaseAction> actions, object contributer);

        void CommitPendingChanges(Action successAction = null);

        void ClearPendingChanges();

        void ClearMatchingPendingChanges(Predicate<DatabaseAction> predicate);

        bool HasPendingChanges { get; }

        ObservableCollection<DatabaseAction> PendingChanges { get; }
    }

    public interface IChangeContainerObserver {

        void OnChangesCommitted();

    }
}
