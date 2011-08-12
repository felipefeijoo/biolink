using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Data {

    /// <summary>
    /// Marker attribute to indicate to Change Detection routines to ignore formatting only changes
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreRTFFormattingChanges : Attribute {
    }
}
