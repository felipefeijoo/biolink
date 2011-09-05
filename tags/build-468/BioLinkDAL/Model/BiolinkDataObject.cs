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
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;

namespace BioLink.Data.Model {

    [AttributeUsage(AttributeTargets.Property)]
    public class MappingInfo : System.Attribute {

        public MappingInfo(string column) {
            Column = column;
            Ignore = false;
        }

        public bool Ignore { get; set; }
        public string Column { get; set; }
    }

    // Place holder class representing all Biolink Transfer Objects (TO)    
    [Serializable()]
    public abstract class BioLinkDataObject  {

        protected BioLinkDataObject() {            
        }

        protected abstract Expression<Func<int>> IdentityExpression { get; }

        public int? ObjectID {
            get {
                if (IdentityExpression == null) {
                    return null;
                } else {
                    return GetExpressionValue(IdentityExpression);
                }
            }
        }


        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            throw new NotImplementedException();
        }

        protected T GetExpressionValue<T>(Expression<Func<T>> expr, T @default = default(T)) {
            if (expr == null) {
                return @default;
            } else {
                var destProp = (PropertyInfo)((MemberExpression)expr.Body).Member;
                return (T)destProp.GetValue(this, null);
            }
        }

    }



    // Biolink data objects that have a GUID column
    [Serializable()]
    public abstract class GUIDObject : BioLinkDataObject {      

        public Nullable<Guid> GUID { get; set; }
    }

    // Biolink data objects that have ownership columns
    [Serializable()]
    public abstract class OwnedDataObject : GUIDObject {

        public DateTime DateCreated { get; set; }
        public string WhoCreated { get; set; }
        public DateTime DateLastUpdated { get; set; }
        public string WhoLastUpdated { get; set; }

    }

}
