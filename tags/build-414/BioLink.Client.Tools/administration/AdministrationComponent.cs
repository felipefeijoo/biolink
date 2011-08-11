using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Windows;


namespace BioLink.Client.Tools {

    public class AdministrationComponent : UserControl, ILazyPopulateControl {

        public AdministrationComponent() {
        }


        public AdministrationComponent(User user) {
            this.User = user;
        }

        public bool IsPopulated { get; protected set; }

        public virtual void Populate() {
            throw new NotImplementedException();
        }

        protected User User { get; private set; }

        protected SupportService Service { get { return new SupportService(User); } }

        protected void RegisterPendingChange(DatabaseCommand command) {
            var changeContainer = FindChangeContainer();
            if (changeContainer != null) {
                changeContainer.RegisterPendingChange(command, this);
            }
        }

        protected void RegisterUniquePendingChange(DatabaseCommand command) {
            var changeContainer = FindChangeContainer();
            if (changeContainer != null) {
                changeContainer.RegisterUniquePendingChange(command, this);
            }
        }

        private IChangeContainer FindChangeContainer() {
            var p = this as FrameworkElement;

            while (!(p is IChangeContainer) && p != null) {
                p = p.Parent as FrameworkElement;
            }

            if (p != null) {
                return (IChangeContainer)p;
            }
            return null;
        }


    }
}
