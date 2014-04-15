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
using System.Collections;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    public class MatrixColumn {
        public string Name { get; set; }
        public bool IsHidden { get; set; }
    }

    public class VirtualMatrixColumn : MatrixColumn {
        
        public System.Func<MatrixRow, Object> ValueGenerator { get; set; }

        public object GetValue(MatrixRow row) {
            if (ValueGenerator != null) {
                return ValueGenerator(row);
            }
            return null;
        }
    }

    public class DataMatrix : IEnumerable {

        public Object Tag { get; set; }

        public DataMatrix() {
            this.Columns = new List<MatrixColumn>();
            this.Rows = new List<MatrixRow>();
        }

        public List<MatrixColumn> Columns { get; set; }
        public List<MatrixRow> Rows { get; set; }

        public MatrixRow AddRow() {
            var row = new MatrixRow(this, new object[Columns.Count]);
            Rows.Add(row);
            return row;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new GenericEnumerator(Rows.ToArray());
        }

        public int IndexOf(string columnName) {
            int index = 0;
            foreach (MatrixColumn col in Columns) {
                if (col.Name == columnName) {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public bool ContainsColumn(string columnName) {
            foreach (var col in Columns) {
                if (col.Name == columnName) {
                    return true;
                }
            }
            return false;
        }

        public void RemoveRow(int rowIndex) {
            if (Rows != null) {
                Rows.RemoveAt(rowIndex);
            }
        }

        public MatrixRow FindRow(string columnName, object value) {
            var index = IndexOf(columnName);
            if (index >= 0) {
                return FindRow((row) => {
                    var candidateValue = row[index];
                    return  candidateValue == null ? false : candidateValue.Equals(value);
                });
            }

            return null;
        }

        public MatrixRow FindRow(Predicate<MatrixRow> predicate) {
            if (predicate != null) {
                foreach (MatrixRow candidate in Rows) {
                    if (predicate(candidate)) {
                        return candidate;
                    }
                }
            };

            return null;
        }

        public void AppendMatrix(DataMatrix chunk) {            
            chunk.Rows.ForEach(row => {
                var newrow = this.AddRow();
                for (int colIndex = 0; colIndex < Columns.Count; colIndex++) {
                    newrow[colIndex] = row[colIndex];
                }
            });
        }
    }

    public class MatrixRow {

        private object[] _data;
        private DataMatrix _matrix;

        internal MatrixRow(DataMatrix matrix, object[] data) {
            _matrix = matrix;
            _data = data;
        }

        public Object this [int index] {
            get {

                if (index < 0) {
                    throw new IndexOutOfRangeException();
                }

                if (index >= _data.Length) {
                    // This might be a virtual column...
                    if (index < _matrix.Columns.Count) {
                        VirtualMatrixColumn vcol = _matrix.Columns[index] as VirtualMatrixColumn;
                        if (vcol != null) {
                            return vcol.GetValue(this);
                        }                            
                    }
                    // If we get here, something bad has happened!
                    throw new IndexOutOfRangeException();
                }

                return _data[index];
            }
            set { _data[index] = value; }
        }

        public Object this [string columnName] {
            get { 
                var index = _matrix.IndexOf(columnName);
                if (index >= 0) {
                    return this[index];
                }
                return null;
            }
            set {
                var index = _matrix.IndexOf(columnName);
                if (index >= 0) {
                    this[index] = value;
                }
            }
        }

        public int Count {
            get { return _matrix.Columns.Count; }
        }

        public object First(Predicate<object> predicate) {
            for (int i = 0; i < _matrix.Columns.Count; ++i) {
                object val = this[i];
                if (predicate(val)) {
                    return val;
                }
            }
            return null;
        }

        public void ForEach(System.Func<int, object, bool> func) {
            for (int i = 0; i < _matrix.Columns.Count; ++i) {
                object val = this[i];
                if (!func(i, val)) {
                    break;
                }
            }
        }


        public DataMatrix Matrix { 
            get { return _matrix; } 
        }

    }

    class GenericEnumerator : IEnumerator {
        private readonly object[] _list;
        // Enumerators are positioned before the first element
        // until the first MoveNext() call. 

        private int _position = -1;

        public GenericEnumerator(object[] list) {
            _list = list;
        }

        public bool MoveNext() {
            _position++;
            return (_position < _list.Length);
        }

        public void Reset() {
            _position = -1;
        }

        public object Current {
            get {
                try { return _list[_position]; } catch (IndexOutOfRangeException) { throw new InvalidOperationException(); }
            }
        }
    }

}
