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
using System.IO;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for QueryTool.xaml
    /// </summary>
    public partial class QueryTool : UserControl {

        private ObservableCollection<QueryCriteria> _model;
        private List<FieldDescriptor> _fields;
        private bool _distinct = false;

        static QueryTool() {
            AddCriteria = new RoutedCommand("AddCriteriaCommand", typeof(QueryTool));
            RemoveCriteria = new RoutedCommand("RemoveCriteriaCommand", typeof(QueryTool));
            RemoveAllCriteria = new RoutedCommand("RemoveAllCriteriaCommand", typeof(QueryTool));
            MoveCriteriaUp = new RoutedCommand("MoveCriteriaUpCommand", typeof(QueryTool));
            MoveCriteriaDown = new RoutedCommand("MoveCriteriaDownCommand", typeof(QueryTool));
            NewQuery = new RoutedCommand("NewQueryCommand", typeof(QueryTool));
            OpenQuery = new RoutedCommand("OpenQueryCommand", typeof(QueryTool));
            SaveQuery = new RoutedCommand("SaveQueryCommand", typeof(QueryTool));
            ShowSQL = new RoutedCommand("ShowSQLCommand", typeof(QueryTool));
            ExecuteQuery = new RoutedCommand("ExecuteQueryCommand", typeof(QueryTool));

        }

        public QueryTool(User user, ToolsPlugin owner) {

            this.CommandBindings.Add(new CommandBinding(AddCriteria, ExecutedAddCriteria, CanExecuteAddCriteria));
            this.CommandBindings.Add(new CommandBinding(RemoveCriteria, ExecutedRemoveCriteria, CanExecuteRemoveCriteria));
            this.CommandBindings.Add(new CommandBinding(RemoveAllCriteria, ExecutedRemoveAllCriteria, CanExecuteRemoveAllCriteria));
            this.CommandBindings.Add(new CommandBinding(MoveCriteriaUp, ExecutedMoveCriteriaUp, CanExecuteMoveCriteriaUp));
            this.CommandBindings.Add(new CommandBinding(MoveCriteriaDown, ExecutedMoveCriteriaDown, CanExecuteMoveCriteriaDown));
            this.CommandBindings.Add(new CommandBinding(NewQuery, ExecutedNewQuery, CanExecuteNewQuery));
            this.CommandBindings.Add(new CommandBinding(OpenQuery, ExecutedOpenQuery, CanExecuteOpenQuery));
            this.CommandBindings.Add(new CommandBinding(SaveQuery, ExecutedSaveQuery, CanExecuteSaveQuery));
            this.CommandBindings.Add(new CommandBinding(ShowSQL, ExecutedShowSQL, CanExecuteShowSQL));
            this.CommandBindings.Add(new CommandBinding(ExecuteQuery, ExecutedExecuteQuery, CanExecuteExecuteQuery));

            ExecuteQuery.InputGestures.Add(new KeyGesture(Key.F5, ModifierKeys.Control));

            InitializeComponent();
            this.User = user;
            this.Owner = owner;

            var service = new SupportService(user);
            _fields = service.GetFieldMappings();
            lvwFields.ItemsSource = _fields;

            CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lvwFields.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            myView.GroupDescriptions.Add(groupDescription);

            _model = new ObservableCollection<QueryCriteria>();
            criteriaGrid.ItemsSource = _model;

            var sortItems = new List<String>(new string[] { CriteriaSortConverter.NOT_SORTED, "Ascending", "Descending" });
            sortColumn.ItemsSource = sortItems;

        }

        public ToolsPlugin Owner { get; private set; }

        public User User { get; private set; }

        private void lvwFields_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            txtDescription.DataContext = lvwFields.SelectedItem;
        }

        private void AddCriteriaImpl() {

            var field = lvwFields.SelectedItem as FieldDescriptor;
            if (field != null) {
                var c = new QueryCriteria { Field = field, Output = true };
                _model.Add(c);
            }
        }

        private void RemoveCriteriaImpl() {
            var c = criteriaGrid.SelectedItem as QueryCriteria;
            if (c != null) {
                _model.Remove(c);
            }
        }

        private void RemoveAllCriteriaImpl() {
            _model.Clear();
        }

        private void MoveCriteriaUpImpl() {
            int oldIndex = criteriaGrid.SelectedIndex;
            if (oldIndex > 0) {
                _model.Move(oldIndex, oldIndex - 1);
            }                
        }

        private void MoveCriteriaDownImpl() {
            int oldIndex = criteriaGrid.SelectedIndex;
            if (oldIndex >= 0 && oldIndex < _model.Count - 1) {
                _model.Move(oldIndex, oldIndex + 1);
            }
        }

        private void NewQueryImpl() {
            if (_model.Count > 0) {
                if (!this.Question("Are you sure you wish to discard the current query and create a new one?", "Discard query?")) {
                    return;
                }
            }
            _model.Clear();
        }

        private void OpenQueryImpl() {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Load Query";
            dlg.DefaultExt = "blq"; // Default file extension
            dlg.Filter = "Query Files (*.blq)|*.blq|All files (*.*)|*.*"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) {                
                _model = LoadQueryFile(dlg.FileName);
                criteriaGrid.ItemsSource = _model;
            }
            
        }

        private ObservableCollection<QueryCriteria> LoadQueryFile(string filename) {
            var model = new ObservableCollection<QueryCriteria>();
            using (var reader = new StreamReader(filename)) {
                string strLine = reader.ReadLine(); // Skip first line (contains header...);
                string expected = string.Format("Field{0}Criteria{0}Output{0}Alias{0}Sort", (char)2);
                if (strLine != expected) {
                    throw new Exception("Invalid query file. Header mismatch.");
                }

                int lineCount = 0;
                while ((strLine = reader.ReadLine()) != null) {
                    if (strLine.Trim().Length > 0) {
                        lineCount++;
                        String[] bits = strLine.Split((char)2);
                        if (bits.Length != 5) {
                            throw new Exception("Invalid query file. Incorrect number of delimiters on line " + lineCount);
                        }

                        // first find the field given its category/name combo...
                        var field = FindFieldByLongName(bits[0]);
                        if (field != null) {
                            var c = new QueryCriteria { Field = field, Criteria = bits[1], Output = (bits[2].Trim() == "1"), Alias = bits[3], Sort = bits[4] };
                            model.Add(c);
                        } else {
                            // Could not locate the field...what to do? Ignore or throw?
                        }
                    }
                }
            }
            return model;
        }

        private FieldDescriptor FindFieldByLongName(string name) {
            int index = name.IndexOf(".");
            if (index > 0) {
                string category = name.Substring(0, index);
                string displayName = name.Substring(index + 1);

                foreach (FieldDescriptor f in _fields) {
                    if (f.Category == category && f.DisplayName == displayName) {
                        return f;
                    }
                }
            }

            return null;
        }

        private void SaveQueryImpl() {
            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Title = "Save Query";
            dlg.FileName = "query"; // Default file name
            dlg.DefaultExt = "blq"; // Default file extension
            dlg.OverwritePrompt = true;
            dlg.Filter = "Query Files (*.blq)|*.blq|All files (*.*)|*.*"; // Filter files by extension
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) {
                SaveQueryFile(_model, dlg.FileName);                
            }
        }

        private void SaveQueryFile(ObservableCollection<QueryCriteria> model, string filename) {
            using (StreamWriter writer = new StreamWriter(filename)) {
                writer.WriteLine(string.Format("Field{0}Criteria{0}Output{0}Alias{0}Sort", (char)2));
                foreach (QueryCriteria c in model) {
                    writer.WriteLine(string.Format("{1}.{2}{0}{3}{0}{4}{0}{5}{0}{6}", (char)2, c.Field.Category, c.Field.DisplayName, c.Criteria, c.Output ? "1" : "0", c.Alias, c.Sort));
                }
            }
        }

        private void ShowSQLImpl() {
            var service = new SupportService(User);
            var sql = service.GenerateQuerySQL(_model, _distinct);
            var frm = new SQLViewer(sql);
            frm.Owner = this.FindParentWindow();
            frm.ShowDialog();            
        }

        private void ExecuteQueryImpl() {

            try {
                var report = new QueryReport(User, _model, _distinct);
                ReportResults results = new ReportResults(report);
                PluginManager.Instance.AddDocumentContent(this.Owner, results, report.Name);            
            } catch (Exception ex) {
                ErrorMessage.Show(ex.Message);
            }

        }

        #region Commands

        public static RoutedCommand AddCriteria { get; private set; }

        private void CanExecuteAddCriteria(object sender, CanExecuteRoutedEventArgs e) {

            QueryTool target = e.Source as QueryTool;

            if (target != null) {
                e.CanExecute = target.lvwFields.SelectedItem != null;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedAddCriteria(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.AddCriteriaImpl();
            }
        }

        public static RoutedCommand RemoveCriteria { get; private set; }

        private void CanExecuteRemoveCriteria(object sender, CanExecuteRoutedEventArgs e) {

            QueryTool target = e.Source as QueryTool;

            if (target != null) {
                e.CanExecute = target.criteriaGrid.SelectedItem != null;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedRemoveCriteria(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.RemoveCriteriaImpl();
            }
        }

        public static RoutedCommand RemoveAllCriteria { get; private set; }

        private void CanExecuteRemoveAllCriteria(object sender, CanExecuteRoutedEventArgs e) {

            QueryTool target = e.Source as QueryTool;

            if (target != null) {
                e.CanExecute = target.criteriaGrid.Items.Count > 0;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedRemoveAllCriteria(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.RemoveAllCriteriaImpl();
            }
        }

        public static RoutedCommand MoveCriteriaUp { get; private set; }

        private void CanExecuteMoveCriteriaUp(object sender, CanExecuteRoutedEventArgs e) {

            QueryTool target = e.Source as QueryTool;

            if (target != null) {
                e.CanExecute = target.criteriaGrid.SelectedIndex > 0;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedMoveCriteriaUp(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.MoveCriteriaUpImpl();
            }
        }

        public static RoutedCommand MoveCriteriaDown { get; private set; }

        private void CanExecuteMoveCriteriaDown(object sender, CanExecuteRoutedEventArgs e) {

            QueryTool target = e.Source as QueryTool;

            if (target != null) {
                e.CanExecute = target.criteriaGrid.SelectedIndex < target._model.Count - 1 && target.criteriaGrid.SelectedIndex >= 0;
            } else {
                e.CanExecute = false;
            }
        }

        private void ExecutedMoveCriteriaDown(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.MoveCriteriaDownImpl();
            }
        }

        public static RoutedCommand NewQuery { get; private set; }

        private void CanExecuteNewQuery(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void ExecutedNewQuery(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.NewQueryImpl();
            }
        }

        public static RoutedCommand OpenQuery { get; private set; }

        private void CanExecuteOpenQuery(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = true;
        }

        private void ExecutedOpenQuery(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.OpenQueryImpl();
            }
        }

        public static RoutedCommand SaveQuery { get; private set; }

        private void CanExecuteSaveQuery(object sender, CanExecuteRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                e.CanExecute = target._model.Count > 0;
            } else {
                e.CanExecute = true;
            }
        }

        private void ExecutedSaveQuery(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.SaveQueryImpl();
            }
        }

        public static RoutedCommand ShowSQL { get; private set; }

        private void CanExecuteShowSQL(object sender, CanExecuteRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                e.CanExecute = target._model.Count > 0;
            } else {
                e.CanExecute = true;
            }
        }

        private void ExecutedShowSQL(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.ShowSQLImpl();
            }
        }

        public static RoutedCommand ExecuteQuery { get; private set; }

        private void CanExecuteExecuteQuery(object sender, CanExecuteRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                e.CanExecute = target._model.Count > 0;
            } else {
                e.CanExecute = true;
            }
        }

        private void ExecutedExecuteQuery(object sender, ExecutedRoutedEventArgs e) {
            QueryTool target = e.Source as QueryTool;
            if (target != null) {
                target.ExecuteQueryImpl();
            }
        }
        #endregion

        private void lvwFields_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            AddCriteriaImpl();
        }

        private void txtFilter_TypingPaused(string text) {
            ApplyFilter(text);
        }

        private void ApplyFilter(string text) {

            ListCollectionView dataView = CollectionViewSource.GetDefaultView(lvwFields.ItemsSource) as ListCollectionView;

            if (String.IsNullOrEmpty(text)) {
                dataView.Filter = null;
                return;
            }
            
            text = text.ToLower();
            dataView.Filter = (obj) => {
                var field = obj as FieldDescriptor;

                if (field != null) {
                    if (field.DisplayName.ToLower().Contains(text)) {
                        return true;
                    }

                    if (field.FieldName.ToLower().Contains(text)) {
                        return true;
                    }

                    if (field.TableName.ToLower().Contains(text)) {
                        return true;
                    }

                    return false;
                }
                return true;
            };

            dataView.Refresh();
        }

    }

    internal class CriteriaSortConverter : IValueConverter {

        public const string NOT_SORTED = "(Not sorted)";

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var s = value as string;
            if (string.IsNullOrEmpty(s)) {
                return NOT_SORTED;
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var s = value as string;
            if (s == NOT_SORTED) {
                return "";
            }
            return value;
        }

    }

    internal class QueryFieldConverter : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var fld = value as FieldDescriptor;
            if (fld != null) {
                return string.Format("{0}.{1}", fld.Category, fld.DisplayName);
            }
            return "!";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    internal class QueryReport : ReportBase {

        public QueryReport(User user, IEnumerable<QueryCriteria> criteria, bool distinct) : base(user) {
            this.Criteria = criteria;
            this.Distinct = distinct;
            RegisterViewer(new TabularDataViewerSource());
        }

        public override string Name {
            get { return "Query Results"; }
        }

        public override DataMatrix ExtractReportData(IProgressObserver progress) {
            var service = new SupportService(User);
            return service.ExecuteQuery(Criteria, Distinct);
        }

        public IEnumerable<QueryCriteria> Criteria { get; private set; }
        public bool Distinct { get; private set; }
    }

}


