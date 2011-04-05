using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using BioLink.Client.Utilities;

namespace BioLink.Data.Model {

    [Serializable()]
    public class Trait : GUIDObject{

        public int TraitID { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string Comment { get; set; }

        public int IntraCatID { get; set; }        

        public string DataType { get; set; }

        public string Validation { get; set; }

        public string Category { get; set; }

        protected override Expression<Func<int>> IdentityExpression {
            get { return () => this.TraitID; }
        }

    }

    public class TraitCategory : GUIDObject {

        public int TraitCategoryID { get; set; }

        public string Category { get; set; }

        protected override Expression<Func<int>> IdentityExpression {
            get { return () => this.TraitCategoryID; }
        }

    }

    public class TraitType : GUIDObject {
        public int TraitTypeID { get; set; }
        public string TraitTypeName { get; set; }
        public string DataType { get; set; }
        public string ValidationStr { get; set; }
        public int CategoryID { get; set; }

        protected override Expression<Func<int>> IdentityExpression {
            get { return () => this.TraitTypeID; }
        }
    }

    public class TypeData : BioLinkDataObject {

        public int ID { get; set; }
        [MappingInfo("Type")]
        public string Description { get; set; }
        public string Category { get; set; }        

        protected override Expression<Func<int>> IdentityExpression {
            get { return () => this.ID; }
        }
    }

    public abstract class TypeDataOwnerInfo : BioLinkDataObject {

        protected abstract Expression<Func<int>> TypeDataIDExpression { get; }
        protected abstract Expression<Func<string>> ValueExpression { get; }
        protected abstract Expression<Func<string>> RTFExpression { get; }

        public int TypeDataID {
            get { return GetExpressionValue(TypeDataIDExpression); }
        }

        public string TypeDataValue {
            get { return GetExpressionValue(ValueExpression); }
        }

        public string RTF {
            get { return GetExpressionValue(RTFExpression); }
        }

        public string Category { get; set; }
        public int OwnerID { get; set; }
        public string OwnerName { get; set; }
        public string OwnerCall { get; set; }
        public string EntryPoint { get; set; }        

    }

    public class TraitOwnerInfo : TypeDataOwnerInfo {

        protected override Expression<Func<int>> IdentityExpression {
            get { return () => this.TraitID; }
        }

        protected override Expression<Func<int>> TypeDataIDExpression {
            get { return () => this.TraitTypeID; }
        }

        protected override Expression<Func<string>> ValueExpression {
            get { return () => this.Value; }
        }

        protected override Expression<Func<string>> RTFExpression {
            get { return () => this.Comment; }
        }

        public int TraitID { get; set; }
        public int TraitTypeID { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }

    }


    public class NoteOwnerInfo : TypeDataOwnerInfo {

        protected override Expression<Func<int>> IdentityExpression {
            get { return () => this.NoteID; }
        }

        protected override Expression<Func<int>> TypeDataIDExpression {
            get { return () => this.NoteTypeID; }
        }

        protected override Expression<Func<string>> ValueExpression {
            get { return () => this.NoteLength; }
        }

        protected override Expression<Func<string>> RTFExpression {
            get { return () => this.Note; }
        }


        public int NoteID { get; set; }
        public int NoteTypeID { get; set; }
        public string Note { get; set; }
        
        public string NoteLength {
            get { return ByteConverter.FormatBytes(Note == null ? 0 : Note.Length); }
        }

    }


}
