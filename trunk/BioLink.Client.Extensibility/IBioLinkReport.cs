using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Data;

namespace BioLink.Client.Extensibility {

    public interface IBioLinkReport {

        string Name { get; }
        List<IReportViewerSource> Viewers { get; }
        DataTable ExtractReportData();

    }

    public interface IReportViewerSource {
        string Name { get; }
        FrameworkElement ConstructView(DataTable reportData, IProgressObserver progress);
    }

}
