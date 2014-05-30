using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BioLink.Client.Extensibility {

    public static class TransformFactory {

        private static Dictionary<String, ValueTransformer> _registry;

        static TransformFactory() {
            _registry = new Dictionary<string, ValueTransformer>();
            // Register all the diffrent types of transformers
            register(new UpperCaseTransformer());
            register(new LowerCaseTransformer());
            register(new ParseDateTransformer());
        }

        static void register(ValueTransformer prototype) {
            _registry.Add(prototype.Key, prototype);
        }

        public static ValueTransformer CreateTransform(String key, Object config = null) {
            ValueTransformer result = null;
            if (!String.IsNullOrEmpty(key)) {
                if (_registry.ContainsKey(key)) {
                    // Create a new one from the type of the prototype in the registry
                    result = Activator.CreateInstance(_registry[key].GetType()) as ValueTransformer;
                    if (config != null) {
                        result.RestoreFromConfiguration(config);
                    }
                }
            }
            return result;
        }

        public static List<ValueTransformer> Transforms {
            get {
                var list = new List<ValueTransformer>();
                foreach (ValueTransformer t in _registry.Values) {
                    list.Add(t);
                }
                list.Sort( (t1, t2) => { return String.Compare(t1.Name, t2.Name); });
                return list;
            }
        }
        
    }
}
