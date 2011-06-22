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
using BioLink.Client.Utilities;
using BioLink.Client.Extensibility;
using BioLink.Data;
using BioLink.Data.Model;
using System.Collections.ObjectModel;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for LoanSearchControl.xaml
    /// </summary>
    public partial class LoanSearchControl : DatabaseActionControl {

        private Point _startPoint;
        private bool _IsDragging;
        private ObservableCollection<LoanViewModel> _model;

        public LoanSearchControl(User user, ToolsPlugin plugin) : base( user, "LoanSearch") {
            InitializeComponent();
            this.Plugin = plugin;

            Loaded += new RoutedEventHandler(LoanSearchControl_Loaded);

            txtFind.PreviewKeyDown += new KeyEventHandler(txtFind_PreviewKeyDown);

            lvw.MouseRightButtonUp += new MouseButtonEventHandler(lvw_MouseRightButtonUp);

            lvw.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lvw_PreviewMouseLeftButtonDown);
            lvw.PreviewMouseMove += new MouseEventHandler(lvw_PreviewMouseMove);

        }

        void lvw_PreviewMouseMove(object sender, MouseEventArgs e) {
            CommonPreviewMouseMove(e, sender as ListView);
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


        void lvw_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(lvw);
        }

        void lvw_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            var builder = new ContextMenuBuilder(null);

            builder.New("Edit Loan").Handler(() => { EditSelectedLoan(); }).End();
            builder.Separator();
            builder.New("Delete Loan").Handler(() => { DeleteSelectedLoan(); }).End();
            builder.Separator();
            builder.New("Add New Loan").Handler(() => { AddNewLoan(); }).End();

            lvw.ContextMenu = builder.ContextMenu;

        }

        void txtFind_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                e.Handled = true;
                DoFind();
            }
        }

        private void DoFind() {

            var service = new LoanService(User);

            var what = cmbWhat.SelectedItem as Pair<string, string>;
            if (string.IsNullOrWhiteSpace(txtFind.Text) || what == null) {
                lvw.ItemsSource = new ObservableCollection<LoanViewModel>();
                return;
            }

            var list = service.FindLoans(txtFind.Text, what.Second, chkFindOnlyOpenLoans.IsChecked.ValueOrFalse());
            _model = new ObservableCollection<LoanViewModel>(list.Select((m) => {
                return new LoanViewModel(m);
            }));

            lvw.ItemsSource = _model;
        }

        protected Pair<string, string> Pair(string a, string b) {
            return new Pair<string, string>(a, b);
        }

        void LoanSearchControl_Loaded(object sender, RoutedEventArgs e) {
            var options = new Pair<string, string>[] { Pair("Find in all", "A"), Pair("Loan number", "L"), Pair("Permit number", "P"), /* Pair("Taxon name", "T") */ };

            cmbWhat.ItemsSource = options;
            cmbWhat.DisplayMemberPath = "First";
            cmbWhat.SelectionChanged += new SelectionChangedEventHandler(cmbWhat_SelectionChanged);

            cmbWhat.SelectedIndex = Config.GetProfile<int>(User, "FindLoans.LastFindWhat", 0);

        }

        void cmbWhat_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Config.SetProfile(User, "FindLoans.LastFindWhat", cmbWhat.SelectedIndex);
        }

        protected ToolsPlugin Plugin { get; private set; }

        private void btnAddNew_Click(object sender, RoutedEventArgs e) {
            AddNewLoan();
        }

        private void btnProperties_Click(object sender, RoutedEventArgs e) {
            EditSelectedLoan();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            DeleteSelectedLoan();
        }

        private void AddNewLoan() {
            Plugin.AddNewLoan();
        }

        private LoanViewModel GetSelectedLoan() {
            return lvw.SelectedItem as LoanViewModel;
        }

        private void EditSelectedLoan() {
            var selected = GetSelectedLoan();
            if (selected != null) {
                Plugin.EditLoan(selected.LoanID);
            }
        }

        private void DeleteSelectedLoan() {

            var loan = GetSelectedLoan();
            if (loan == null) {
                return;
            }

            loan.IsDeleted = true;
            _model.Remove(loan);
            RegisterUniquePendingChange(new DeleteLoanAction(loan.Model));
        }

        private void btnFind_Click(object sender, RoutedEventArgs e) {
            DoFind();
        }

        private void lvw_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DependencyObject src = (DependencyObject)(e.OriginalSource);
            while (!(src is Control)) {
                src = VisualTreeHelper.GetParent(src);
            }

            if (src != null && src is ListViewItem) {
                if (lvw.SelectedItem != null) {
                    EditSelectedLoan();
                }
            }

        }



    }
}
