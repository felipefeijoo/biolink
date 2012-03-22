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

namespace BioLink.Client.Utilities {

    public delegate bool ProgressHandler(string message, double percentComplete, ProgressEventType progressEventType);

    /// <summary>
    /// Things that can display status messages, including and indication of percent completion
    /// </summary>
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
