using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

    //public abstract class MapPointSetGenerator : IMapPointSetGenerator {

    //    public abstract MapPointSet GeneratePoints();

    //}

    public interface IMapPointSetGenerator {

        MapPointSet GeneratePoints(bool showOptions);

    }

    public class DelegatingPointSetGenerator<T> : IMapPointSetGenerator where T : class {

        private Func<bool, T, MapPointSet> _delegate;
        private T _arg;

        public DelegatingPointSetGenerator(Func<bool, T, MapPointSet> @delegate, T arg) {
            _delegate = @delegate;
            _arg = arg;
        }

        public MapPointSet GeneratePoints(bool showOptions) {
            if (_delegate != null) {
                return _delegate(showOptions, _arg);
            }
            return null;
        }
    }
}
