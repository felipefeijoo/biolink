using System;

namespace BioLink.Client.Extensibility {

    public class DockableContentOptions {

        public DockableContentOptions() {
            IsClosable = true;
            IsFloating = false;
        }

        public String Title { get; set; }  
        public bool IsClosable { get; set; }
        public bool IsFloating { get; set; }

    }
}
