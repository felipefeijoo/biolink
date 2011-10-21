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
using System.Diagnostics;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Utility class used during debugging to measure the duration of a method or operation
    /// </summary>
    public class CodeTimer : IDisposable {

        /* thing that is atually used to measure the time */
        private readonly Stopwatch _stopwatch;
        /* What to do when the timer is stopped */
        private readonly StopAction _stopAction;

        /// <summary>
        /// Create a new timer with the supplied name, and an optional action to perform when the timer is stopped
        /// </summary>
        /// <param name="name">The name of the timer</param>
        /// <param name="stopAction">An action to be executed when the timer is stopped</param>
        public CodeTimer(string name, StopAction stopAction = null) {
            Name = name;
            _stopAction = stopAction ?? DefaultStopAction;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        /// <summary>
        /// Stop the code timer
        /// </summary>
        public void Stop() {
            _stopwatch.Stop();
            if (_stopAction != null) {
                _stopAction(Name, _stopwatch.Elapsed);                
            } 
        }

        /// <summary>
        /// Start/Restart the timer
        /// </summary>
        public void Start() {           
            _stopwatch.Start();
        }

        /// <summary>
        /// Reset the code timer
        /// </summary>
        public void Reset() {
            _stopwatch.Reset();
        }

        public void Restart() {
            _stopwatch.Restart();            
        }

        public void Dispose() {
            Stop();            
        }

        #region Properties

        /// <summary>
        /// The action to perform when the timer is stopped. Is static, and can be set globally during application startup
        /// </summary>
        public static StopAction DefaultStopAction { get; set; }

        /// <summary>
        /// Name of the code timer (e.g. Method name, or some description)
        /// </summary>
        public String Name { get; private set; }

        #endregion

        #region Delegates

        /// <summary>
        /// Delegate describing an action to be performed when the time stops
        /// </summary>
        /// <param name="name">The name of the timer stopping</param>
        /// <param name="elapsed">The time elapsed time since the timer was started</param>
        public delegate void StopAction(string name, TimeSpan elapsed);

        #endregion
    }


}
