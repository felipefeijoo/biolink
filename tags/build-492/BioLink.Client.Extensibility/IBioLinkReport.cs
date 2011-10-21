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
using System.Windows;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public interface IBioLinkReport {

        string Name { get; }
        bool DisplayOptions(User user, Window parentWindow);
        List<IReportViewerSource> Viewers { get; }
        DataMatrix ExtractReportData(IProgressObserver progress);
        List<DisplayColumnDefinition> DisplayColumns { get; }

    }

    public class DisplayColumnDefinition {

        public DisplayColumnDefinition() {
            this.DisplayFormat = "{0}";
        }

        public string ColumnName { get; set; }
        public string DisplayName { get; set; }
        public string DisplayFormat { get; set; }

    }

    public interface IReportViewerSource {
        string Name { get; }
        FrameworkElement ConstructView(IBioLinkReport report, DataMatrix reportData, IProgressObserver progress);
    }

}
