using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data {

    public interface IXMLImportProgressObserver {

        void ProgressMessage(string message);

        void ImportStarted(string message, List<XMLImportProgressItem> items);

        void ImportCompleted();

        void ProgressTick(string itemName, int countCompleted);
    }

    public class XMLImportProgressItem {
        public string Name { get; set; }
        public int Total { get; set; }
        public int Completed { get; set; }
    }
}
