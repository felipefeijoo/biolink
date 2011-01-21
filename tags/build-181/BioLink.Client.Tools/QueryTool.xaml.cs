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
    /// Interaction logic for QueryTool.xaml
    /// </summary>
    public partial class QueryTool : UserControl {

        private ObservableCollection<QueryCriteria> _model;

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
            var mappings = service.GetFieldMappings();
            lvwFields.ItemsSource = mappings;

            CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lvwFields.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            myView.GroupDescriptions.Add(groupDescription);

            _model = new ObservableCollection<QueryCriteria>();
            criteriaGrid.ItemsSource = _model;

            var sortItems = new List<String>(new string[] { "(No sort)", "Ascending", "Descending" });
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
            MessageBox.Show("New Query!");
        }

        private void OpenQueryImpl() {
            MessageBox.Show("Open Query!");
        }

        private void SaveQueryImpl() {
            MessageBox.Show("Save Query!");
        }

        private void ShowSQLImpl() {
            MessageBox.Show("Show SQL!");
        }

        private void ExecuteQueryImpl() {
            MessageBox.Show("Execute Query!");
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

    }

    public class QueryCriteria {
        public FieldDescriptor Field { get; set; }
        public string Criteria { get; set; }
        public bool Output { get; set; }
        public string Alias { get; set; }
        public string Sort { get; set; }
    }
}
