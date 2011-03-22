using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {

    public class JournalViewModel : GenericViewModelBase<Journal> {

        public JournalViewModel(Journal model) : base(model, ()=>model.JournalID) {
            DataChanged += new DataChangedHandler(JournalViewModel_DataChanged);
        }

        void JournalViewModel_DataChanged(ChangeableModelBase viewmodel) {
            RaisePropertyChanged("DisplayLabel");
        }

        protected override string RelativeImagePath {
            get {
                return @"images\Journal.png";
            }
        }

        public override string DisplayLabel {
            get { return FullName; }
        }

        public int JournalID {
            get { return Model.JournalID; }
            set { SetProperty(()=>Model.JournalID, value); }
        }

        public string AbbrevName { 
            get { return Model.AbbrevName; }
            set { SetProperty(()=>Model.AbbrevName, value); }
        }

        public string AbbrevName2 {
            get { return Model.AbbrevName2; }
            set { SetProperty(() => Model.AbbrevName2, value); }
        }

        public string Alias {
            get { return Model.Alias; }
            set { SetProperty(() => Model.Alias, value); }
        }

        public string FullName {
            get { return Model.FullName; }
            set { SetProperty(() => Model.FullName, value); }
        }

        public string Notes {
            get { return Model.Notes; }
            set { SetProperty(() => Model.Notes, value); }
        }

    }
}
