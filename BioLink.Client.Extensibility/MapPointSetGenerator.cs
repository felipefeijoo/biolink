/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BioLink.Client.Extensibility {

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
