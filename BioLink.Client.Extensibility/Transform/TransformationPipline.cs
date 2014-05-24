using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class TransformationPipline {

        private List<IValueTransformer> _pipeline = new List<IValueTransformer>();

        public TransformationPipline() {
        }

        public String transform(String value, IRowSource row) {
            String transformed = value;
            foreach (IValueTransformer t in _pipeline) {
                transformed = t.Transform(transformed, row);
            }
            return transformed;
        }

        public void AddTransformer(IValueTransformer transform) {
            _pipeline.Add(transform);
        }

        public List<IValueTransformer> Transformers {
            get { return _pipeline; }
        }


        public override String ToString() {            
            List<String> l = new List<String>();            
            foreach (IValueTransformer t in _pipeline) {
                l.Add(t.Name);                
            }
            return l.Join(" → ");
        }
    }
}
