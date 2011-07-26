using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Utilities {

    public class Pair<TK, TV> {

        public Pair() { }

        public Pair(TK first, TV second) {
            First = first;
            Second = second;
        }

        public TK First { get; set; }
        public TV Second { get; set; }
    }

}
