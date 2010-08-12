using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Same as Queue except Dequeue function blocks until there is an object to return.
    /// </summary>
    public class BlockingQueue<T> {

        private Queue<T> _queue = new Queue<T>();

        public void Enqueue(T element) {
            _queue.Enqueue(element);
            lock (_queue) {
                Monitor.Pulse(_queue);
            }
        }

        public T Deqeue() {
            lock (_queue) {
                while (_queue.Count == 0) {
                    Monitor.Wait(_queue);
                }
                return _queue.Dequeue();
            }
        }

        public void Clear() {
            lock (_queue) {
                _queue.Clear();
            }
        }


        public int Count {
            get {
                lock (_queue) {
                    return _queue.Count;
                }
            }
        }

    }
}
