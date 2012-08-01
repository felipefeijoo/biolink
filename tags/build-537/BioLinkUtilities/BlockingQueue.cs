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

using System.Collections.Generic;
using System.Threading;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Same as Queue except Dequeue function blocks until there is an object to return.
    /// </summary>
    public class BlockingQueue<T> {

        private readonly Queue<T> _queue = new Queue<T>();

        /// <summary>
        /// Push a new element to the tail of the queue
        /// </summary>
        /// <param name="element">Item to place in the queue</param>
        public void Enqueue(T element) {
            _queue.Enqueue(element);
            lock (_queue) {
                Monitor.Pulse(_queue);
            }
        }

        /// <summary>
        /// Take the next item from the head of the queue. If no items exists, calling thread will block until something is added.
        /// </summary>
        /// <returns>The next item</returns>
        public T Deqeue() {
            lock (_queue) {
                while (_queue.Count == 0) {
                    Monitor.Wait(_queue);
                }
                return _queue.Dequeue();
            }
        }

        /// <summary>
        /// Clears the queue of all elements
        /// </summary>
        public void Clear() {
            lock (_queue) {
                _queue.Clear();
            }
        }

        /// <summary>
        /// The number of elements in the queue
        /// </summary>
        public int Count {
            get {
                lock (_queue) {
                    return _queue.Count;
                }
            }
        }

    }
}
