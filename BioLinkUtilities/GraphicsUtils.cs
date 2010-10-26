using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Utilities {
    public static class GraphicsUtils {

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource SystemDrawingImageToBitmapSource(System.Drawing.Image image) {
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image)) {
                IntPtr hBitmap = bitmap.GetHbitmap();
                System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(hBitmap);
                return bitmapSource;
            }
        }

        public static System.Drawing.Drawing2D.HatchStyle? GetHatchStyleFromBrush(System.Drawing.Brush brush) {
            if (brush is System.Drawing.Drawing2D.HatchBrush) {
                return (brush as System.Drawing.Drawing2D.HatchBrush).HatchStyle;
            }
            return null;
        }

        public static System.Drawing.Color GetColorFromBrush(System.Drawing.Brush brush) {

            if (brush is System.Drawing.SolidBrush) {
                return (brush as System.Drawing.SolidBrush).Color;
            }

            if (brush is System.Drawing.Drawing2D.HatchBrush) {
                return (brush as System.Drawing.Drawing2D.HatchBrush).ForegroundColor;
            }

            throw new Exception("Unhandled brush type! Could not extract brush color");
        }

        public static System.Drawing.Brush CreateBrush(System.Drawing.Color color, System.Drawing.Drawing2D.HatchStyle? hatchStyle) {
            if (hatchStyle != null && hatchStyle.HasValue) {
                return new System.Drawing.Drawing2D.HatchBrush(hatchStyle.Value, color, System.Drawing.Color.FromArgb(0,0,0,0));
            } else {
                return new System.Drawing.SolidBrush(color);
            }
        }
    }
}
