using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BioLink.Client.Extensibility {

    public static class TransformFactory {

        private static Dictionary<String, IValueTransformer> _registry;

        static TransformFactory() {
            _registry = new Dictionary<string, IValueTransformer>();
            register(new UpperCaseTransformer());            
        }

        static void register(IValueTransformer prototype) {
            _registry.Add(prototype.Key, prototype);
        }

        public static IValueTransformer CreateTransform(String key, Object config) {
            IValueTransformer result = null;
            if (!String.IsNullOrEmpty(key)) {
                if (_registry.ContainsKey(key)) {
                    // Create a new one from the type of the prototype in the registry
                    result = Activator.CreateInstance(_registry[key].GetType()) as IValueTransformer;
                    result.RestoreFromConfiguration(config);
                }
            }
            return result;
        }
        
    }
}
