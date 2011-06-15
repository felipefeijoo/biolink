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
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LoansForContact.xaml
    /// </summary>
    public partial class LoansForContact : DatabaseActionControl {

        private ObservableCollection<LoanViewModel> _model;

        public LoansForContact(User user, ToolsPlugin plugin, int contactId) : base(user, "LoansForContact:" + contactId) {
            InitializeComponent();
            Plugin = plugin;
            this.ContactID = contactId;
            LoadModelAsync();

            lvw.MouseDoubleClick += new MouseButtonEventHandler(lvw_MouseDoubleClick);
        }

        void lvw_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var selected = lvw.SelectedItem as LoanViewModel;
            if (selected != null) {
                EditLoan(selected.LoanID);
            }
        }

        private void EditLoan(int loanId) {
            Plugin.EditLoan(loanId);
        }

        private void LoadModelAsync() {
            JobExecutor.QueueJob(() => {
                var service = new LoanService(User);
                var list = service.ListLoansForContact(ContactID);
                _model = new ObservableCollection<LoanViewModel>(list.Select((model) => {
                    return new LoanViewModel(model);
                }));

                lvw.InvokeIfRequired(() => {
                    lvw.ItemsSource = _model;
                });
            });
        }

        protected int ContactID { get; private set; }

        protected ToolsPlugin Plugin { get; private set; }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNewLoan();
        }

        private void AddNewLoan() {
            Plugin.AddNewLoan();
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {
            var loan = GetSelectedLoan();
            if (loan != null) {
                EditLoan(loan.LoanID);
            }
        }

        private LoanViewModel GetSelectedLoan() {
            return lvw.SelectedItem as LoanViewModel;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            var loan = GetSelectedLoan();
            if (loan != null) {
                DeleteLoan(loan);
            }
        }

        private void DeleteLoan(LoanViewModel loan) {
            if (loan == null) {
                return;
            }

            loan.IsDeleted = true;
            _model.Remove(loan);
            RegisterUniquePendingChange(new DeleteLoanAction(loan.Model));
        }

    }

    public class DeleteLoanAction : GenericDatabaseAction<Loan> {

        public DeleteLoanAction(Loan model) : base(model) { }

        protected override void ProcessImpl(User user) {
            var service = new LoanService(user);
            service.DeleteLoan(Model.LoanID);
        }
    }
}
