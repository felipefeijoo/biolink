using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    //public abstract class MapPointSetGenerator : IMapPointSetGenerator {

    //    public abstract MapPointSet GeneratePoints();

    //}

    public interface IMapPointSetGenerator {

        MapPointSet GeneratePoints();

    }

    public class DelegatingPointSetGenerator<T> : IMapPointSetGenerator where T : class {

        private Func<T, MapPointSet> _delegate;
        private T _arg;

        public DelegatingPointSetGenerator(Func<T, MapPointSet> @delegate, T arg) {
            _delegate = @delegate;
            _arg = arg;
        }

        public MapPointSet GeneratePoints() {
            if (_delegate != null) {
                return _delegate(_arg);
            }
            return null;
        }
    }
}
