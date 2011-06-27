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
using System.IO;

namespace BioLink.Client.Tools {
    /// <summary>
    /// Interaction logic for GenerateLoanFormControl.xaml
    /// </summary>
    public partial class GenerateLoanFormControl : Window {

        public GenerateLoanFormControl(User user, ToolsPlugin plugin, int loanId) {
            InitializeComponent();

            this.User = user;
            this.Plugin = plugin;
            this.LoanID = loanId;

            lvw.MouseDoubleClick += new MouseButtonEventHandler(lvw_MouseDoubleClick);

            Loaded += new RoutedEventHandler(GenerateLoanFormControl_Loaded);
        }

        void lvw_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            GenerateSelectedLoanForm();
        }

        void GenerateLoanFormControl_Loaded(object sender, RoutedEventArgs e) {
            var service = new SupportService(User);

            var forms = service.GetMultimediaItems(TraitCategoryType.Biolink.ToString(), SupportService.BIOLINK_INTRA_CAT_ID);
            var model = new ObservableCollection<LoanFormTemplateViewModel>(forms.Select((m) => {
                return new LoanFormTemplateViewModel(m);
            }));

            lvw.ItemsSource = model;
        }

        protected User User { get; private set; }

        protected ToolsPlugin Plugin { get; private set; }

        protected int LoanID { get; set; }

        private void btnManageForms_Click(object sender, RoutedEventArgs e) {
            Plugin.ShowLoanFormManager();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            GenerateSelectedLoanForm();
        }

        private void GenerateSelectedLoanForm() {
            var selected = lvw.SelectedItem as LoanFormTemplateViewModel;
            if (selected != null) {
                GenerateLoanForm(selected.MultimediaID);
            }
        }

        private void GenerateLoanForm(int mmID) {
            var service = new LoanService(User);
            var loan = service.GetLoan(LoanID);
            var loanMaterial = service.GetLoanMaterial(LoanID);
            // var loanCorrespondence = service.GetLoanCorrespondence(LoanID);

            var supportSevice = new SupportService(User);
            var loanTraits = supportSevice.GetTraits(TraitCategoryType.Loan.ToString(), LoanID);
            var bytes = supportSevice.GetMultimediaBytes(mmID);
            var template = Encoding.ASCII.GetString(bytes);
            var content = GenerateLoanForm(template, loan, loanMaterial, loanTraits);

            var filename = TempFileManager.NewTempFilename("RTF", "LoanForm");
            File.WriteAllText(filename, content);
            SystemUtils.ShellExecute(filename);

            this.Close();

        }

        private string GenerateLoanForm(string template, Loan loan, List<LoanMaterial> material, List<Trait> traits) {
            var sb = new StringBuilder();
            var reader = new System.IO.StringReader(template);
            int i;
            while ((i = reader.Read()) >= 0) {
                char ch = (char)i;
                if (ch == '<') {
                    ch = (char) reader.Read();
                    if (ch == '<') {
                        var placeHolder = ReadPlaceHolder(reader);
                        if (!string.IsNullOrEmpty(placeHolder)) {
                            var value = SubstitutePlaceHolder(placeHolder, loan, material, traits);
                            if (!string.IsNullOrEmpty(value)) {
                                sb.Append(value);
                            }
                        }
                    } else {
                        sb.AppendFormat("<{0}", ch);
                    }
                } else {
                    sb.Append(ch);
                }
            }
            
            return sb.ToString();
        }

        private string SubstitutePlaceHolder(string key, Loan loan, List<LoanMaterial> material, List<Trait> traits) {
            var sb = new StringBuilder();            
            if (key.Contains('(')) {
                // group...
                var collectionName = key.Substring(0, key.IndexOf('('));
                var fieldstr = key.Substring(key.IndexOf('(')+1);
                var fields = fieldstr.Substring(0, fieldstr.Length - 1).Split(',');

                List<object> collection = null;                
                if (collectionName.Equals("material", StringComparison.CurrentCultureIgnoreCase)) {
                    collection = new List<object>(material);
                } else if (collectionName.Equals("trait", StringComparison.CurrentCultureIgnoreCase)) {
                    collection = new List<object>(traits);
                }

                if (collection != null) {
                    foreach (Object obj in collection) {
                        int i = 0;
                        foreach (string field in fields) {
                            var value = GetPropertyValue(obj, field);
                            if (!string.IsNullOrEmpty(value)) {
                                sb.Append(RTFUtils.EscapeUnicode(value));
                            }
                            if (++i < fields.Length) {
                                sb.Append(", ");
                            } else {
                                sb.Append(". \\par\\pard ");
                            }
                        }
                    }
                }
                
            } else {                
                // single value from the Loan model...
                var value = GetPropertyValue(loan, key);
                if (!string.IsNullOrEmpty(value)) {                    
                    sb.Append(RTFUtils.EscapeUnicode(value));
                }
            }
            return sb.ToString();
        }

        private string GetPropertyValue(object obj, string propertyName) {

            var p = obj.GetType().GetProperty(propertyName);
            if (p == null) {
                var prefixes = MapperBase.KNOWN_TYPE_PREFIXES.Split(',');
                foreach (string prefix in prefixes) {
                    if (propertyName.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase)) {
                        propertyName = propertyName.Substring(prefix.Length);
                        p = obj.GetType().GetProperty(propertyName);
                        break;
                    }
                }
            }
            if (p != null) {
                var val = p.GetValue(obj, null);
                if (val != null) {
                    if (val is DateTime) {
                        return string.Format("{0:d}", val);
                    } 
                    return val.ToString();
                }
            }

            return null;
        }

        private string ReadPlaceHolder(TextReader reader) {
            var sb = new StringBuilder();
            bool finished = false;
            int i;
            while ((!finished && (i = reader.Read()) >= 0)) {
                char ch = (char)i;
                if (ch == '>') {
                    ch = (char)reader.Read();
                    if (ch == '>') {
                        finished = true;
                    } else {
                        sb.AppendFormat("<{0}", ch);
                    }
                } else {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
        }

    }

    public class LoanFormTemplateViewModel : MultimediaLinkViewModel {

        public LoanFormTemplateViewModel(MultimediaLink model) : base(model) { }

        public string FileDesc {
            get { return string.Format("{0} {1}", this.Extension, SystemUtils.ByteCountToSizeString(this.SizeInBytes)); }
        }
    }

}
