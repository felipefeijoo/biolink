using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public interface IClosable : IDisposable {

        bool RequestClose();

        bool HasPendingChanges { get; }

        void ApplyChanges();

        event PendingChangedRegisteredHandler PendingChangedRegistered;

        event PendingChangesCommittedHandler PendingChangesCommitted;

    }

    public delegate void PendingChangedRegisteredHandler(object sender, object action);

    public delegate void PendingChangesCommittedHandler(object sender);

}
