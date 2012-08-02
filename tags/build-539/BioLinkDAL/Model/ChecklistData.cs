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

namespace BioLink.Data.Model {

    public class ChecklistData {

        public int BiotaID { get; set; }
        public int IndentLevel { get; set; }
        public string DisplayName { get; set; }
        public string Author { get; set; }
        public string Type { get; set; }
        public string Rank { get; set; }
        public bool Verified { get; set; }        
        public bool Unplaced { get; set; }
        public bool AvailableName { get; set; }
        public bool LiteratureName { get; set; }
        public string RankCategory { get; set; }

    }
}
