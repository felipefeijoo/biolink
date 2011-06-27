using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using BioLink.Data.Model;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LabelManagerControl.xaml
    /// </summary>
    public partial class LabelManagerControl : OneToManyDetailControl {

        public LabelManagerControl(User user) : base(user, "LabelManager") {
            InitializeComponent();
        }

        public override FrameworkElement FirstControl {
            get { return lvw;  }
        }

        public override List<ViewModelBase> LoadModel() {
            var service = new SupportService(User);
            var list = service.GetLabelSets();
            return new List<ViewModelBase>(list.Select((model) => {
                return new LabelSetViewModel(model);
            }));            
        }

        public override DatabaseAction PrepareDeleteAction(ViewModelBase viewModel) {
            return new DeleteLabelSetAction((viewModel as LabelSetViewModel).Model);
        }

        public override DatabaseAction PrepareUpdateAction(ViewModelBase viewModel) {
            return new UpdateLabelSetAction((viewModel as LabelSetViewModel).Model);
        }

        public override ViewModelBase AddNewItem(out DatabaseAction addAction) {
            var model = new LabelSet { Name = "New set" };
            addAction = new InsertLabelSetAction(model);
            return new LabelSetViewModel(model);
        }

    }

    public class LabelSetViewModel : GenericViewModelBase<LabelSet> {

        public LabelSetViewModel(LabelSet model) : base(model, () => model.ID) { }

        public override string DisplayLabel {
            get { return string.Format("{0}", Name); }
        }

        public int ID {
            get { return Model.ID; }
            set { SetProperty(() => Model.ID, value); }
        }

        public string Name {
            get { return Model.Name; }
            set { SetProperty(() => Model.Name, value); }
        }

        public string Delimited {
            get { return Model.Delimited; }
            set { SetProperty(() => Model.Delimited, value); }
        }

    }

    public class DeleteLabelSetAction: GenericDatabaseAction<LabelSet> {

        public DeleteLabelSetAction(LabelSet model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.DeleteLabelSet(Model.ID);
        }
    }

    public class InsertLabelSetAction : GenericDatabaseAction<LabelSet> {

        public InsertLabelSetAction(LabelSet model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            Model.ID = service.InsertLabelSet(Model);
        }
    }

    public class UpdateLabelSetAction : GenericDatabaseAction<LabelSet> {
        public UpdateLabelSetAction(LabelSet model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new SupportService(user);
            service.UpdateLabelSet(Model);
        }
    }

}
