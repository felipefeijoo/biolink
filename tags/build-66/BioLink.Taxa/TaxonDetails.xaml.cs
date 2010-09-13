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
using BioLink.Data.Model;
using System.Reflection;
using BioLink.Data;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Taxa {
    /// <summary>
    /// Interaction logic for TaxonDetails.xaml
    /// </summary>
    public partial class TaxonDetails : DatabaseActionControl<TaxaService> {

        #region designer constructor
        public TaxonDetails() {
            InitializeComponent();
        }
        #endregion

        public TaxonDetails(TaxonViewModel taxon, TaxaService service) : base(service, "TaxonDetails::" + taxon.TaxaID.Value) {
            InitializeComponent();           

            AddTabItem("General", new TaxonNameDetails(taxon.TaxaID, service));            
            AddTabItem("Traits", new TraitControl(service.User, TraitCategoryType.Taxon, taxon.TaxaID));
            var mmc = new MultimediaControl(service.User, TraitCategoryType.Taxon, taxon.TaxaID);
            AddTabItem("Multimedia", mmc, () => {
                if (!mmc.IsPopulated) {
                    mmc.PopulateControl();
                }
            });
            AddTabItem("Ownership", new OwnershipDetails(taxon.Taxon));

            this.Taxon = taxon;
            // Build dynamic content...

            PropertyInfo[] props = Taxon.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in props) {
                Grid item = new Grid();
                item.ColumnDefinitions.Add(new ColumnDefinition());
                item.ColumnDefinitions.Add(new ColumnDefinition());
                TextBlock block = new TextBlock();

                block.Text = prop.Name + ":";                
                item.Children.Add(block);
                Grid.SetColumn(block, 0);

                TextBox txt = new TextBox();
                object value = prop.GetValue(Taxon, null);
                txt.Text = (value == null ? "" : value.ToString());

                item.Children.Add(txt);
                Grid.SetColumn(txt, 1);

                this.contentStack.Children.Add(item);

            }

        }

        private TabItem AddTabItem(string title, UIElement content, Action bringIntoViewAction = null) {            
            TabItem tabItem = new TabItem();
            tabItem.Header = title;
            tabItem.Content = WireUpContent(content);
            tabControl.Items.Add(tabItem);
            if (bringIntoViewAction != null) {
                tabItem.RequestBringIntoView += new RequestBringIntoViewEventHandler((s, e) => {
                    bringIntoViewAction();
                });
            }
                 
            return tabItem;
        }

        public UIElement WireUpContent(UIElement element) {
            if (element is IClosable) {
                var closable = element as IClosable;                
                closable.PendingChangedRegistered += new PendingChangedRegisteredHandler((source, a) => {
                    if (PendingChanges.Count == 0) {
                        RegisterPendingAction(new GenericDatbaseAction<TaxaService>((service) => {
                            foreach (TabItem item in tabControl.Items) {
                                if (item.Content is DatabaseActionControl<TaxaService>) {
                                    var control = item.Content as DatabaseActionControl<TaxaService>;
                                    if (control.HasPendingChanges) {
                                        control.ApplyChanges();
                                    }
                                } else if (item.Content is DatabaseActionControl<SupportService>) {
                                    var control = item.Content as DatabaseActionControl<SupportService>;
                                    if (control.HasPendingChanges) {
                                        control.ApplyChanges();
                                    }
                                }
                            }
                        }));
                    }
                });
            }
            return element;
        }

        #region properties

        public TaxonViewModel Taxon { get; private set; }

        #endregion

    }



}
