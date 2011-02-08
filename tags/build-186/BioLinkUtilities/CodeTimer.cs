using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace BioLink.Client.Utilities {

    public class CodeTimer : IDisposable {

        public static StopAction DefaultStopAction { get; set; }

        public String Name { get; private set; }
        private Stopwatch _stopwatch;
        private StopAction _stopAction;

        public CodeTimer(string name, StopAction stopAction = null) {
            Name = name;
            if (stopAction != null) {
                _stopAction = stopAction;
            } else {
                _stopAction = DefaultStopAction;
            }
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public void Stop() {
            _stopwatch.Stop();
            if (_stopAction != null) {
                _stopAction(Name, _stopwatch.Elapsed);                
            } 
        }

        public void Start() {           
            _stopwatch.Start();
        }

        public void Reset() {
            _stopwatch.Reset();
        }

        public void Restart() {
            _stopwatch.Restart();            
        }

        public delegate void StopAction(string name, TimeSpan elapsed);


        public void Dispose() {
            Stop();            
        }
    }


}
