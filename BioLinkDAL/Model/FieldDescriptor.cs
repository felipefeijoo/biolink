using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;

namespace BioLink.Data.Model {

    public class FieldDescriptor {

        public string DisplayName { get; set; }
        public string FieldName { get; set; }
        public string TableName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Format { get; set; }
        public bool UseInRDE { get; set; }
        public string DataType { get; set; }
        public Func<string, ConvertingValidatorResult, bool> Validate { get; set; }

        public override string ToString() {
            return string.Format("{0} ({1}.{2})", DisplayName, TableName, FieldName);
        }

    }

    public class ConvertingValidatorResult {

        public ConvertingValidatorResult(FieldDescriptor field, string originalValue) {
            this.Field = field;
            this.OriginalValue = originalValue;
        }

        public string OriginalValue { get; private set; }
        public bool IsValid { get; private set; }
        public object ConvertedValue { get; private set; }
        public string Message { get; private set; }
        public FieldDescriptor Field { get; private set; }

        public bool Fail(string format, params object[] args) {
            if (args != null && args.Length > 0) {
                Message = String.Format("{0}.{1}: {3} ('{2}')", Field.Category, Field.DisplayName, OriginalValue.Truncate(30), string.Format(format, args));
            } else {
                Message = String.Format("{0}.{1}: {3} ('{2}')", Field.Category, Field.DisplayName, OriginalValue.Truncate(30), format);
            }
            IsValid = false;
            ConvertedValue = null;
            return false;
        }

        public bool Success(object convertedValue, string message = null) {
            IsValid = true;
            ConvertedValue = convertedValue;
            Message = message;
            return true;
        }
    }

    public class QueryCriteria {

        public FieldDescriptor Field { get; set; }
        public string Criteria { get; set; }
        public bool Output { get; set; }
        public string Alias { get; set; }
        public string Sort { get; set; }
        public string FormatOption { get; set; }

    }

}
