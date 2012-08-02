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

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Helper class providing useful methods for debugging
    /// </summary>
    public class Debug {

        /// <summary>
        /// Assert a particular predicate is true. If false (and in DEBUG mode), and exception is thrown
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="message"></param>
        public static void Assert(bool predicate, string message = "Assertion failed!") {
#if (DEBUG)
                if (predicate == false) {
                    Logger.Debug("Predicate Assertion failed! {0}", message);
                    throw new AssertionFailedException(message);
                }
#endif
        }
        
        /// <summary>
        /// Assert that an object is not null. If null (and DEBUG is defined) an exception is thrown.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="message"></param>
        public static void AssertNotNull(object obj, string message = "Object is null!") {
#if (DEBUG)
            if (obj == null) {
                throw new AssertionFailedException(message);
            }
#endif
        }

    }

    /// <summary>
    /// Exception class to indicate a failed Assertion
    /// </summary>
    public class AssertionFailedException : Exception {
        public AssertionFailedException(string message) : base(message) { }
    }
}
