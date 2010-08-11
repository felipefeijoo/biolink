using BioLink.Client.Utilities;
using BioLink.Data;
using System.Windows;

namespace BioLink.Client.Extensibility {

    public interface ITabularDataExporter<TOptionType> : IBioLinkExtension {

        TOptionType GetOptions(Window parentWindow);

        void Export(DataMatrix matrix, TOptionType options, IProgressObserver progress);

        #region Properties

        string Description { get; }

        #endregion

    }
}
