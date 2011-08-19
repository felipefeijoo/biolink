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
using System.Windows.Shapes;
using System.Windows.Resources;
using System.IO;

namespace BioLinkApplication {
    /// <summary>
    /// Interaction logic for ThirdPartyLicenses.xaml
    /// </summary>
    public partial class ThirdPartyLicenses : Window {
        public ThirdPartyLicenses() {
            InitializeComponent();

            var model = new List<Credit>();

            model.Add(new Credit { 
                Name = "Avalon Dock", 
                Description = "Window docking framework for WPF", 
                Url = "http://avalondock.codeplex.com/", 
                LicenseType= "Modified BSD", 
                LicenseTemplate = "NewBSD",
                CopyrightName = "Adolfo Marinucci",
                CopyrightYear= "2007-2009"
            });

            model.Add(new Credit { 
                Name = "SharpMap", 
                Description = "Open source GIS library for Windows", 
                Url = "http://sharpmap.codeplex.com/", 
                LicenseType = "Lesser GPL", 
                LicenseTemplate = "LGPL",
                CopyrightName = "",
                CopyrightYear= ""
            });

            model.Add(new Credit {
                Name = "FWTools",
                Description = "Open Source GIS Binary Kit for Windows and Linux. Used by BioLink to add raster capabilities to SharpMap.",
                Url = "http://fwtools.maptools.org/",
                LicenseType = "MIT",
                LicenseTemplate = "MIT",
                CopyrightName = "Frank Warmerdam",
                CopyrightYear = "2008"
            });

            model.Add(new Credit {
                Name = "Microsoft Enterprise Library 5.0",
                Description = "Application framework and infrastructure libary. Mainly used by BioLink for logging.",
                Url = "http://msdn.microsoft.com/en-us/library/ff650810.aspx",
                LicenseType = "Microsoft Public License",
                LicenseTemplate = "MsPL",
                CopyrightName = "Microsoft",
                CopyrightYear = "2010"
            });

            model.Add(new Credit {
                Name = "Json.NET",
                Description = "Utility library for serializing and deserializing objects using the Javascript Object Notation (JSON) format",
                Url = "http://james.newtonking.com/pages/json-net.aspx",
                LicenseType = "MIT",
                LicenseTemplate = "MIT",
                CopyrightName = "James Newton-King",
                CopyrightYear = "2007"
            });

            model.Add(new Credit {
                Name = "System.Data.SQLite",
                Description = "Data access library providing the ability to create, query and modify SQLite databases. Used by BioLink for the configuration store, eGaz and Import/Export.",
                Url = "http://system.data.sqlite.org/",
                LicenseType = "Public Domain (No License)",
                LicenseTemplate = "",
                CopyrightName = "",
                CopyrightYear = ""
            });

            model.Add(new Credit {
                Name = "\"GenericParser\"",
                Description = "Flat file parsing library, used by BioLink for the importing of value seperated files.",
                Url = "http://www.codeproject.com/KB/database/GenericParser.aspx",
                LicenseType = "MIT",
                LicenseTemplate = "MIT",
                CopyrightName = "Andrew Rissing",
                CopyrightYear = "2005"
            });

            foreach (Credit credit in model) {
                var ctl = new ThirdPartyComponentControl(credit);
                credits.Children.Add(ctl);
            }
        }
    }

    public class Credit {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string LicenseType { get; set; }
        public string LicenseTemplate { get; set; }
        public string CopyrightYear { get; set; }
        public string CopyrightName { get; set; }

        public string LicenseText {
            get {
                string txt = "No license";
                string assemblyName = this.GetType().Assembly.GetName().Name;
                if (!string.IsNullOrWhiteSpace(LicenseTemplate)) {
                    string packUri = String.Format("pack://application:,,,/{0};component/licenses/{1}.txt", assemblyName, LicenseTemplate);
                    Uri uri = new Uri(packUri, UriKind.Absolute);
                    StreamResourceInfo info = Application.GetResourceStream(uri);
                    if (info != null) {
                        using (var reader = new StreamReader(info.Stream)) {
                            txt = reader.ReadToEnd();
                            txt = txt.Replace("${CopyrightYear}", CopyrightYear);
                            txt = txt.Replace("${CopyrightName}", CopyrightName);
                        }
                    }
                }
                return txt;
            }
        }
    }
}
