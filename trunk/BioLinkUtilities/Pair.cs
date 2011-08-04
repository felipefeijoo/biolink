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

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Generic Pair class (Key=Value)
    /// </summary>
    /// <typeparam name="TK">The Key type</typeparam>
    /// <typeparam name="TV">The Value type</typeparam>
    public class Pair<TK, TV> {

        public Pair() { }

        public Pair(TK first, TV second) {
            First = first;
            Second = second;
        }

        public TK First { get; set; }
        public TV Second { get; set; }
    }

}
