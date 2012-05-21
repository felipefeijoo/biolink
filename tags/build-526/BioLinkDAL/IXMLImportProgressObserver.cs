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

namespace BioLink.Data {

    public interface IXMLImportProgressObserver {

        void ProgressMessage(string message);

        void ImportStarted(string message, List<XMLImportProgressItem> items);

        void ImportCompleted();

        void ProgressTick(string itemName, int countCompleted);
    }

    public class XMLImportProgressItem {
        public string Name { get; set; }
        public int Total { get; set; }
        public int Completed { get; set; }
    }
}
