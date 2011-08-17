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
using System.Threading;

namespace BioLink.Client.Extensibility {

    /// <summary>
    /// Facade over thread pool, just in case we want to take more control over the threading later
    /// </summary>
    public class JobExecutor {

        public static void QueueJob(WaitCallback job, object state) {
            ThreadPool.QueueUserWorkItem(job, state);
        }

        public static void QueueJob(Action job) {
            ThreadPool.QueueUserWorkItem((ignored) => {
                try {
                    job();
                } catch (Exception ex) {
                    GlobalExceptionHandler.Handle(ex);
                }
            });
        }

    }
}
