using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Data;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public interface IBioLinkReport {

        string Name { get; }
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
