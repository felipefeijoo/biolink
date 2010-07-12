using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public interface IProgressMonitor {
        void ProgressStart(string message);
        void ProgressMessage(string message, int percentComplete);
        void ProgressEnd(string message);
    }

    public enum ProgressEventType {
        Start,
        Update,
        End
    }

    public delegate bool ProgressHandler(string message, int percentComplete, ProgressEventType progressEventType);

    
}
