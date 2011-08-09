using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public interface IOneToManyDetailController {

        List<ViewModelBase> LoadModel();

        ViewModelBase AddNewItem(out DatabaseCommand addAction);

        DatabaseCommand PrepareDeleteAction(ViewModelBase viewModel);

        DatabaseCommand PrepareUpdateAction(ViewModelBase viewModel);

        User User { get; }

        ViewModelBase Owner { get; set; }

        OneToManyControl Host { get; set; }

        bool AcceptDroppedPinnable(PinnableObject pinnable);

        void PopulateFromPinnable(ViewModelBase viewModel, PinnableObject pinnable);

        UIElement GetDetailEditor(ViewModelBase selectedItem);
    }

    public interface IOneToManyDetailEditor {
        UIElement FirstControl { get; }
    }

    public class OneToManyControllerEditor : UserControl, IOneToManyDetailEditor, IOneToManyDetailController {

        protected OneToManyControllerEditor(User user) {
            this.User = user;
        }

        public virtual List<ViewModelBase> LoadModel() {
            throw new NotImplementedException();
        }

        public virtual ViewModelBase AddNewItem(out DatabaseCommand addAction) {
            throw new NotImplementedException();
        }

        public virtual DatabaseCommand PrepareDeleteAction(ViewModelBase viewModel) {
            throw new NotImplementedException();
        }

        public virtual DatabaseCommand PrepareUpdateAction(ViewModelBase viewModel) {
            throw new NotImplementedException();
        }

        public virtual UIElement FirstControl {
            get { return null; }
        }

        public virtual bool AcceptDroppedPinnable(PinnableObject pinnable) {
            return false;
        }

        public virtual void PopulateFromPinnable(ViewModelBase viewModel, PinnableObject pinnable) {
            throw new NotImplementedException();
        }

        public virtual UIElement GetDetailEditor(ViewModelBase selectedItem) {
            return this;
        }

        public User User { get; protected set; }

        public ViewModelBase Owner { get; set; }

        public OneToManyControl Host { get; set; }

    }

}
