using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows.Interop;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Windows;
using System.Runtime.InteropServices;

namespace BioLink.Client.Extensibility {

    public class GoogleEarth {

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(int hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(int hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        public static bool IsInstalled() {
            try {

                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\GoogleEarth.ApplicationGE");
                return key != null;
                
            } catch (Exception) {
                return false;
            }
        }

        public static void GeoTag(GeoCodingAction geoCodeAction) {
            try {                                

                var ge = (dynamic) Interaction.CreateObject("GoogleEarth.ApplicationGE");

                if (ge != null) {

                    int gHandle = ge.GetMainHwnd();
                    SetForegroundWindow(gHandle);

                    // The KMZ file is a zip file containing a reticle png and kml file that will place the cross hairs on the google earth view (via temporary locations)...
                    string kmzFile = PluginManager.Instance.ResourceTempFileManager.ProxyResource(new Uri("pack://application:,,,/BioLink.Client.Extensibility;component/Resources/Target.kmz"));

                    ge.OpenKmlFile(kmzFile, true);

                    var dlg = new GoogleEarthGeoTag(() => {
                        var point = (dynamic)ge.GetPointOnTerrainFromScreenCoords(0, 0);
                        double lat = point.Latitude;
                        double lon = point.Longitude;
                        double alt = point.Altitude;
                        if (geoCodeAction != null) {
                            geoCodeAction(lat, lon, (int)alt);
                        }
                    });
                    
                    // Position the dialog in the lower left region of the GE window...
                    RECT rect;                    
                    GetWindowRect(gHandle, out rect);
                    dlg.Top = rect.Bottom - (dlg.Height + 80);
                    dlg.Left = rect.Right - (dlg.Width + 40);
                    // This will suspend the thread until the dialog is closed...
                    dlg.ShowDialog();

                    // Geocoded or not, if we get here we need to remove our cross hair...
                    // Hokey way of removing the cross hair feature...but there does not seem to be a way to do it via the COM interface
                    try {
                        for (int i = 0; i < 5; i++) {
                            var rmvFeature = ge.GetFeatureByName("BioLink Geo Coding");

                            if (rmvFeature == null) {
                                break;
                            }

                            rmvFeature.Highlight();

                            SetForegroundWindow(gHandle);
                            System.Windows.Forms.SendKeys.SendWait("{DELETE}");
                            System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                        }
                    } catch (Exception) {
                        // ignore
                    }

                }

            } catch (Exception ex) {
                GlobalExceptionHandler.Handle(ex);
            }
        }

    }

    public delegate void GeoCodingAction(double latitude, double longitude, int? elevation);
}
