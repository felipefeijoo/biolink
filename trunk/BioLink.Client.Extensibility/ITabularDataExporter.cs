using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public interface ITabularDataExporter {

        void Export(DataMatrix matrix, IProgressObserver progress);

        #region Properties

        string Name { get; }

        string Description { get; }

        #endregion

    }
}
