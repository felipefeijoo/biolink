using System.Collections.Generic;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Taxa {

    public abstract class TaxonReportBase : IBioLinkReport {

        private List<IReportViewerSource> _viewers = new List<IReportViewerSource>();
        private List<DisplayColumnDefinition> _columnDefinitions = new List<DisplayColumnDefinition>();

        public TaxonReportBase(User user, TaxonViewModel taxon) {
            this.Service = new TaxaService(user);
            this.Taxon = taxon;
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

        public TaxaService Service { get; private set; }

        public TaxonViewModel Taxon { get; private set; }

        public abstract string Name { get; }

        public List<DisplayColumnDefinition> DisplayColumns {
            get { return _columnDefinitions; }
        }

        #endregion

        public abstract DataMatrix ExtractReportData(IProgressObserver progress);

        public virtual bool DisplayOptions(User user, System.Windows.Window parentWindow) {
            return true;
        }

    }
}
