using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public class OneToManyDetailControl : UserControl {

        #region Designer ctor
        public OneToManyDetailControl() {            
        }
        #endregion


        public OneToManyDetailControl(User user, string contentId) {
            this.User = user;            
            this.ContentID = contentId;
        }

        public virtual ViewModelBase AddNewItem(out DatabaseAction addAction) {
            throw new NotImplementedException();
        }


        public virtual DatabaseAction PrepareDeleteAction(ViewModelBase viewModel) {
            throw new NotImplementedException();
        }

        public virtual List<ViewModelBase> LoadModel() {
            throw new NotImplementedException();
        }

        public virtual DatabaseAction PrepareUpdateAction(ViewModelBase viewModel) {
            throw new NotImplementedException();
        }

        public virtual FrameworkElement FirstControl {
            get {throw new NotImplementedException(); }
        }

        #region Properties

        public string ContentID { get; private set; }

        public User User { get; private set; }

        public ViewModelBase Owner { get; set; }

        public OneToManyControl Host { get; set; }

        #endregion


        public virtual bool AcceptDroppedPinnable(PinnableObject pinnable) {
            return false;
        }

        public virtual void PopulateFromPinnable(ViewModelBase viewModel, PinnableObject pinnable) {
        }

    }
}
