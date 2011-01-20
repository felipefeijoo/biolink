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
    /// Interaction logic for QueryTool.xaml
    /// </summary>
    public partial class QueryTool : UserControl {

        public QueryTool(User user, ToolsPlugin owner) {
            InitializeComponent();
            this.User = user;
            this.Owner = owner;

            var service = new SupportService(user);
            var mappings = service.GetFieldMappings();
            lvwFields.ItemsSource = mappings;

            CollectionView myView = (CollectionView)CollectionViewSource.GetDefaultView(lvwFields.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
            myView.GroupDescriptions.Add(groupDescription);
        }



        public ToolsPlugin Owner { get; private set; }

        public User User { get; private set; }
    }
}
