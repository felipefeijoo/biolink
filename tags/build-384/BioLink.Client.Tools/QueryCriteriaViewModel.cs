using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;



namespace BioLink.Client.Tools {
    public class QueryCriteriaViewModel : GenericViewModelBase<QueryCriteria> {

        public QueryCriteriaViewModel(QueryCriteria model) : base(model, null) { }

        public FieldDescriptor Field {
            get { return Model.Field; }
            set {SetProperty(() => Model.Field, value); }           
        }
        
        public string Criteria {
            get { return Model.Criteria; }
            set { SetProperty(() => Model.Criteria, value); }
        }

        public bool Output {
            get { return Model.Output; }
            set { SetProperty(() => Model.Output, value); }
        }

        public string Alias {
            get { return Model.Alias; }
            set { SetProperty(() => Model.Alias, value); }
        }

        public string Sort {
            get { return Model.Sort; }
            set { SetProperty(() => Model.Sort, value); }
        }

        public string FormatOption {
            get { return Model.FormatOption; }
            set { SetProperty(() => Model.FormatOption, value); }
        }

        public string FieldLabel {
            get { return string.Format("{0}.{1}", Field.Category, Field.DisplayName); }
        }

    }
}
