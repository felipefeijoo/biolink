using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;

namespace BioLink.Client.Extensibility {
    /// <summary>
    /// Interaction logic for TabularDataViewer.xaml
    /// </summary>
    public partial class TabularDataViewer : UserControl {

        #region DesignTime Constructor
        public TabularDataViewer() {
            InitializeComponent();
        }
        #endregion

        public TabularDataViewer(DataTable data) {
            InitializeComponent();
            this.Data = data;
            this.DataContext = data;
        }

        #region Properties

        public DataTable Data { get; private set; }

        #endregion

    }

    public class TabularDataViewerSource : IReportViewerSource {

        public string Name {
            get { return "Table Viewer"; }
        }

        public FrameworkElement ConstructView(DataTable reportData, IProgressObserver progress) {
            TabularDataViewer viewer = new TabularDataViewer(reportData);
            return viewer;            
        }

    }
}
