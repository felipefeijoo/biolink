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
        public List<String> AllowedOptions { get; set; }
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
