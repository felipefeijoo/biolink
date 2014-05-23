using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Utilities;

namespace BioLink.Client.Extensibility {

    public class ImportFieldMapping {
        public string SourceColumn { get; set; }
        public string TargetColumn { get; set; }
        public string DefaultValue { get; set; }
        public bool IsFixed { get; set; }
        public TransformationPipline Transformer { get; set; }

    }

    public class ImportFieldMappingViewModel : GenericViewModelBase<ImportFieldMapping> {

        public ImportFieldMappingViewModel(ImportFieldMapping model) : base(model, () => 0) { }

        public override string DisplayLabel {
            get {
                if (IsFixed) {
                    return string.Format("\"{0}\"", DefaultValue);
                } else {
                    return SourceColumn;
                }
            }
        }

        public String SourceColumn {
            get { return Model.SourceColumn; }
            set { SetProperty(() => Model.SourceColumn, value); }
        }

        public String TargetColumn {
            get { return Model.TargetColumn; }
            set { SetProperty(() => Model.TargetColumn, value); }
        }

        public object DefaultValue {
            get { return Model.DefaultValue; }
            set { SetProperty(() => Model.DefaultValue, value); }
        }

        public bool IsFixed {
            get { return Model.IsFixed; }
            set { SetProperty(() => Model.IsFixed, value); }
        }

        public TransformationPipline Transformer {
            get { return Model.Transformer; }
            set { SetProperty(() => Model.Transformer, value); }
        }

        public void RefreshTransformer() {
            RaisePropertyChanged("Transformer");
        }

    }


}
