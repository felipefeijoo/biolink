using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Interop;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Windows;

namespace BioLink.Client.Extensibility {

    public class GoogleEarth {

        public static bool IsInstalled() {
            try {

                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\GoogleEarth.ApplicationGE");
                return key != null;
                
            } catch (Exception) {
                return false;
            }
        }

        public static void GeoTag(Action<double, double> action) {
            try {
                var ge = (dynamic) Interaction.CreateObject("GoogleEarth.ApplicationGE");

                var dlg = new GoogleEarthGeoTag();            
                if (dlg.ShowDialog() == true) {
                    var point = (dynamic) ge.GetPointOnTerrainFromScreenCoords(0, 0);
                    double lat = point.Latitude;
                    double lon = point.Longitude;
                    if (action != null) {
                        action(lat, lon);
                    }
                }

            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }
        }
    
    }
}
