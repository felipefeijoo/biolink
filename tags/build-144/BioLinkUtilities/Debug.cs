using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Utilities {

    public class Debug {

        public static void Assert(bool predicate, string message = "Assertion failed!") {
#if (DEBUG)
                if (!predicate) {
                    Logger.Debug("Predicate Assertion failed! {0}", message);
                    throw new AssertionFailedException(message);
                }
#endif
        }
        

        public static void AssetNotNull(object obj, string message = "Object is null!") {
#if (DEBUG)
            if (obj == null) {
                throw new AssertionFailedException(message);
            }
#endif
        }

    }

    public class AssertionFailedException : Exception {
        public AssertionFailedException(string message) : base(message) { }
    }
}
