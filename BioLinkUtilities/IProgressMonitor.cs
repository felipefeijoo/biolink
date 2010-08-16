using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Utilities {

    public delegate bool ProgressHandler(string message, double percentComplete, ProgressEventType progressEventType);

    public interface IProgressObserver {
        void ProgressStart(string message, bool indeterminate=false);
        void ProgressMessage(string message, double? percentComplete = null);        
        void ProgressEnd(string message);
    }

    public enum ProgressEventType {
        Start,
        Update,
        End
    }

    public class ProgressObserverAdapter {

        public IProgressObserver _observer;

        public ProgressObserverAdapter(IProgressObserver observer) {
            _observer = observer;
        }

        public bool OnProgress(string message, double percentComplete, ProgressEventType progressEventType) {
            if (_observer != null) {
                switch (progressEventType) {
                    case ProgressEventType.Start:
                        _observer.ProgressStart(message);
                        break;
                    case ProgressEventType.Update:
                        _observer.ProgressMessage(message, percentComplete);
                        break;
                    case ProgressEventType.End:
                        _observer.ProgressEnd(message);
                        break;
                }
            }
            return true;
        }

        public static ProgressHandler Adapt(IProgressObserver observer) {
            return new ProgressObserverAdapter(observer).OnProgress;
        }

    }
    
}
