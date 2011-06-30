using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Utilities {

    public static class ImageCache {

        private static Dictionary<String, BitmapSource> _cache = new Dictionary<string, BitmapSource>();

        public static BitmapSource GetImage(string uri) {

            if (_cache.ContainsKey(uri)) {
                return _cache[uri];
            }

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(uri);
            image.EndInit();
            _cache.Add(uri, image);

            return image;
        }

        public static BitmapSource GetPackedImage(string localpath, string assemblyName) {
            string uri = String.Format("pack://application:,,,/{0};component/{1}", assemblyName, localpath);
            return GetImage(uri);
        }

        public static BitmapSource ApplyOverlay(ImageSource imageSrc, string overlayUri) {

            var image = imageSrc as BitmapSource;

            if (image == null) {
                return null;
            }

            BitmapSource overlay = GetImage(overlayUri);
            int height = (int)image.Height;
            int width = (int)image.Width;
            RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, image.DpiX, image.DpiY, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            drawingContext.DrawImage(image, new Rect(new Point(0, 0), new Point(width, height)));
            drawingContext.DrawImage(overlay, new Rect(new Point(0, 0), new Point(width, height)));
            drawingContext.Close();
            bmp.Render(drawingVisual);
            return bmp;
        }

    }

}
