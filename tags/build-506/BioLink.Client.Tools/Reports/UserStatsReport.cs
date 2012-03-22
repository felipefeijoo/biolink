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
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;


namespace BioLink.Client.Tools {

    public class UserStatsReport : ReportBase {

        public UserStatsReport(User user) : base(user) {
            RegisterViewer(new RTFReportViewerSource());
        }

        public override string Name {
            get { return "Date Entry Statistics by User Report"; }
        }

        public override bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            var frm = new UserStatsReportOptions(user);
            frm.Owner = parentWindow;
            frm.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            if (frm.ShowDialog() == true) {
                Username = frm.Username;
                StartDate = frm.StartDate.ToString("dd MMM yyyy");
                EndDate = frm.EndDate.ToString("dd MMM yyyy");
                return true;
            }

            return false;
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            var service = new SupportService(User);
            return service.GetUserStatisticsReport(Username, StartDate, EndDate);
        }

        public string Username { get; private set; }
        public string StartDate { get; private set; }
        public string EndDate { get; private set; }
    }
}
