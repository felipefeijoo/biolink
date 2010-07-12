using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    public class Debug {

        private static List<IDebugLogObserver> _observers = new List<IDebugLogObserver>();

        public static void RegisterObserver(IDebugLogObserver observer) {
            lock (_observers) {
                if (!_observers.Contains(observer)) {
                    _observers.Add(observer);
                }
            }
        }

        public static void DeregisterObserver(IDebugLogObserver observer) {
            lock (_observers) {
                if (_observers.Contains(observer)) {
                    _observers.Remove(observer);
                }
            }
        }

        public static void Log(Exception ex) {
            Log("Exception: {0}", ex.ToString());
        }

        public static void Log(string format, params object[] args) {
            Log(String.Format(format, args));
        }

        public static void Log(string message) {
            DebugLogMessage msgObj = new DebugLogMessage(message);
            lock (_observers) {
                foreach (IDebugLogObserver observer in _observers) {
                    try {
                        observer.Log(msgObj);
                    } catch (Exception) {
                    }
                }
            }
        }

    }

    public interface IDebugLogObserver {
        void Log(DebugLogMessage message);
    }

    public class DebugLogMessage {

        public string Message {get; private set; }
        public DateTime Date {get; private set; }

        public DebugLogMessage(string message) {
            this.Message = message;
            this.Date = DateTime.Now;
        }
    }

}
