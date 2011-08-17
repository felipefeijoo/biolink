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
using BioLink.Data;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public abstract class ReportBase : IBioLinkReport {

        private List<IReportViewerSource> _viewers = new List<IReportViewerSource>();
        private List<DisplayColumnDefinition> _columnDefinitions = new List<DisplayColumnDefinition>();

        public ReportBase(User user) {
            this.User = user;
        }

        protected void RegisterViewer(IReportViewerSource viewer) {
            _viewers.Add(viewer);
        }

        protected void DefineColumn(DisplayColumnDefinition coldef) {
            _columnDefinitions.Add(coldef);
        }

        protected DisplayColumnDefinition DefineColumn(string name, string display = null) {
            if (display == null) {
                display = name;
            }
            DisplayColumnDefinition coldef = new DisplayColumnDefinition { ColumnName = name, DisplayName = display };
            _columnDefinitions.Add(coldef);
            return coldef;
        }

        public List<IReportViewerSource> Viewers {
            get { return _viewers; }
        }

        #region Properties

        public abstract string Name { get; }

        public List<DisplayColumnDefinition> DisplayColumns {
            get { return _columnDefinitions; }
        }

        public User User { get; private set; }

        #endregion

        public abstract DataMatrix ExtractReportData(IProgressObserver progress);
        
        public virtual bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            return true;
        }
    }
}
