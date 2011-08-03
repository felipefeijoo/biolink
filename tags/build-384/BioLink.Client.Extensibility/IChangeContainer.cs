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
