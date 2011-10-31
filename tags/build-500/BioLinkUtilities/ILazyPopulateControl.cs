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

    /// <summary>
    /// Some controls delay their population until just before they are display, thereby avoiding unnecessary database calls and computation should the control never actually be activated.
    /// </summary>
    public interface ILazyPopulateControl {

        /// <summary>
        /// Has this control populated itself yet?
        /// </summary>
        bool IsPopulated { get; }

        /// <summary>
        /// Allows the control to populate itself
        /// </summary>
        void Populate();

    }
}
