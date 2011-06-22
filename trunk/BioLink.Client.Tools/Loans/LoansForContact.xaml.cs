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
        private Point _startPoint;
        private bool _IsDragging;

        public LoansForContact(User user, ToolsPlugin plugin, int contactId) : base(user, "LoansForContact:" + contactId) {
            InitializeComponent();
            Plugin = plugin;
            this.ContactID = contactId;
            LoadModelAsync();

            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);

            lvw.MouseDoubleClick += new MouseButtonEventHandler(lvw_MouseDoubleClick);

            lvw.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvw_PreviewMouseLeftButtonDown);
            lvw.PreviewMouseMove += new MouseEventHandler(lvw_PreviewMouseMove);
        }

        void lvw_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, sender as ListView);
        }

        void lvw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(lvw);
        }

        private void CommonPreviewMouseMove(MouseEventArgs e, ListView listView) {

            if (_startPoint == null) {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && !_IsDragging) {
                Point position = e.GetPosition(listView);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {

                    var x = listView.InputHitTest(position) as FrameworkElement;
                    if (x != null && x.DataContext is LoanViewModel) {
                        if (listView.SelectedItem != null) {

                            ListViewItem item = listView.ItemContainerGenerator.ContainerFromItem(listView.SelectedItem) as ListViewItem;
                            if (item != null) {
                                StartDrag(e, listView, item);
                            }
                        }
                    }
                }
            }
        }

        private void StartDrag(MouseEventArgs mouseEventArgs, ListView listView, ListViewItem item) {

            var selected = listView.SelectedItem as LoanViewModel;
            if (selected != null) {
                var data = new DataObject("Pinnable", selected);

                var pinnable = new PinnableObject(ToolsPlugin.TOOLS_PLUGIN_NAME, LookupType.Loan, selected.LoanID);
                data.SetData(PinnableObject.DRAG_FORMAT_NAME, pinnable);
                data.SetData(DataFormats.Text, selected.DisplayLabel);

                try {
                    _IsDragging = true;
                    DragDrop.DoDragDrop(item, data, DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link);
                } finally {
                    _IsDragging = false;
                }
            }

            InvalidateVisual();
        }


        void lvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var builder = new ContextMenuBuilder(null);

            builder.New("Edit Loan").Handler(() => { EditSelectedLoan(); }).End();
            builder.Separator();
            builder.New("Delete Loan").Handler(() => { DeleteLoan(GetSelectedLoan()); }).End();
            builder.Separator();
            builder.New("Refresh list").Handler(() => { RefreshContent(); }).End();
            builder.New("Add New Loan").Handler(() => { AddNewLoan(); }).End();

            lvw.ContextMenu = builder.ContextMenu;
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

        private void EditSelectedLoan() {
            var loan = GetSelectedLoan();
            if (loan != null) {
                EditLoan(loan.LoanID);
            }
        }

        private void LoadModelAsync() {
            JobExecutor.QueueJob(() => {
                var service = new LoanService(User);
                var list = service.ListLoansForContact(ContactID);
                _model = new ObservableCollection<LoanViewModel>(list.Select((model) => {
                    return new LoanViewModel(model);
                }));

                this.InvokeIfRequired(() => {
                    lvw.ItemsSource = _model;
                });
            });
        }

        public override void RefreshContent() {
            LoadModelAsync();
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

        private void btnRefresh_Click(object sender, RoutedEventArgs e) {
            RefreshContent();
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
