using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows;
using BioLink.Client.Utilities;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public class UpdateChecker {

        public UpdateCheckResults CheckForUpdates(IProgressObserver progress) {

            // Get the running apps version to compare...
            var v = this.GetType().Assembly.GetName().Version;
            var results = new UpdateCheckResults { CurrentMajor = v.Major, CurrentMinor = v.Minor, CurrentBuild = v.Revision, UpdateExists = false, UpdateLink = "" };
            if (progress != null) {
                progress.ProgressStart("Checking for updates...");
            }
            var updateURL = Config.GetGlobal("BioLink.UpdateURL", "http://code.google.com/feeds/p/biolink/downloads/basic");
            bool updateExists = false;
            try {
                if (progress != null) {
                    progress.ProgressMessage("Contacting update site...");
                }
                var reader = XmlReader.Create(updateURL);
                var feed = SyndicationFeed.Load(reader);

                if (progress != null) {
                    progress.ProgressMessage("Checking update site data...");
                }

                var pattern = new Regex(@"BioLinkInstaller-(\d+)[.](\d+)[.](\d+)[.]exe");
                foreach (SyndicationItem item in feed.Items) {
                    foreach (var link in item.Links) {
                        var m = pattern.Match(link.Uri.AbsoluteUri);
                        if (m.Success) {

                            var major = Int32.Parse(m.Groups[1].Value);
                            var minor = Int32.Parse(m.Groups[2].Value);
                            var build = Int32.Parse(m.Groups[3].Value);

                            if (major > v.Major) {
                                updateExists = true;
                            } else if (major == v.Major) {
                                if (minor > v.Minor) {
                                    updateExists = true;
                                } else if (minor == v.Minor) {
                                    updateExists = build > v.Revision;
                                }
                            }

                            if (updateExists) {
                                results.UpdateMajor = major;
                                results.UpdateMinor = minor;
                                results.UpdateBuild = build;
                                results.UpdateExists = true;
                                results.UpdateLink = link.Uri.AbsoluteUri;

                                return results;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            } finally {
                if (progress != null) {
                    progress.ProgressEnd("Update check complete.");
                }
            }

            return results;
        }
    }

    public class UpdateCheckResults {
        public bool UpdateExists { get; set; }
        public int UpdateMajor { get; set; }
        public int UpdateMinor { get; set; }
        public int UpdateBuild { get; set; }
        public String UpdateLink { get; set; }
        public int CurrentMajor { get; set; }
        public int CurrentMinor { get; set; }
        public int CurrentBuild { get; set; }
    }
}
