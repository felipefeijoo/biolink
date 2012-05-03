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


namespace BioLink.Client.Material {

    public class MaterialForTrapReport : ReportBase {

        public MaterialForTrapReport(User user, SiteExplorerNodeViewModel trap)
            : base(user) {
            this.Trap = trap;

            RegisterViewer(new TabularDataViewerSource());

            DefineColumn("BiotaFullName", "Taxa");
            DefineColumn("FullRegion", "Region");
            DefineColumn("Local", "Locality");
            DefineColumn("FormattedLatLong", "Lat/Long");
            DefineColumn("Collectors");
            DefineColumn("Dates");
            DefineColumn("AccessionNo");

        }

        public override string Name {
            get { return "Material for Trap"; }
        }

        public override Data.DataMatrix ExtractReportData(IProgressObserver progress) {

            var service = new MaterialService(User);

            if (progress != null) {
                progress.ProgressStart(string.Format("Retrieving Material records for Trap {0}", Trap.DisplayLabel), true);
            }

            var serviceMessage = new ServiceMessageDelegate((message) => {
                progress.ProgressMessage(message, 0);
            });

            service.ServiceMessage += serviceMessage;
            DataMatrix matrix = service.GetMaterialForTrap(Trap.ElemID);
            service.ServiceMessage -= serviceMessage;

            if (progress != null) {
                progress.ProgressEnd(string.Format("{0} rows retreived", matrix.Rows.Count));
            }

            matrix.Columns.Add(new FormattedLatLongVirtualColumn(matrix));
            matrix.Columns.Add(new FormattedDateVirtualColumn(matrix));

            return matrix;
        }

        public SiteExplorerNodeViewModel Trap { get; private set; }
    }
}
