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
