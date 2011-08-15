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
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Facade over the enterprise library logging stuff to make it easier...
    /// </summary>
    public static class Logger {

        static Logger() {
            Debug("Logging established");
        }

        public static void Debug(string message, params object[] args) {
            LogImpl("Debug", TraceEventType.Information, message, args);
        }

        public static void Error(string message, params object[] args) {
            LogImpl("Error", TraceEventType.Error, message, args);
        }

        public static void Error(string message, Exception ex) {
            LogImpl("Error", TraceEventType.Error, String.Format("{0}: {1}", message, ex));
        }

        public static void Error(Exception ex) {
            LogImpl("Error", TraceEventType.Error, ex.ToString());
        }

        public static void Warn(string message, params object[] args) {
            LogImpl("Warning", TraceEventType.Warning, message, args);
        }

        private static void LogImpl(string category, TraceEventType severity, string message, params object[] args) {            
            LogEntry entry = new LogEntry(String.Format(message, args), category, 0, 0, severity, "BioLink", null);
            WriteEntry(entry);
        }

        private static void WriteEntry(LogEntry entry) {
            LogWriter writer = EnterpriseLibraryContainer.Current.GetInstance<LogWriter>();
            writer.Write(entry);
        }

        public static LogEntryBuilder Log(string message) {
            return new LogEntryBuilder(message);
        }

        public class LogEntryBuilder : IDisposable {
            private LogEntry _entry;
            public LogEntryBuilder(string message) {
                _entry = new LogEntry();
                _entry.Message = message;
            }

            public LogEntryBuilder Category(string category) {
                _entry.Categories.Add(category);
                return this;
            }

            public LogEntryBuilder Severity(TraceEventType severity) {
                _entry.Severity = severity;
                return this;
            }

            public LogEntryBuilder Title(string title) {
                _entry.Title = title;
                return this;
            }

            public LogEntryBuilder ExtendedProperty(string name, object value) {
                _entry.ExtendedProperties.Add(name, value);
                return this;
            }

            public LogEntryBuilder ManagedThreadName(string name) {
                _entry.ManagedThreadName = name;
                return this;
            }

            public void Send() {
                WriteEntry(_entry);
            }


            public void Dispose() {
                if (_entry != null) {
                    Send();
                }
            }
        }

    }

}
