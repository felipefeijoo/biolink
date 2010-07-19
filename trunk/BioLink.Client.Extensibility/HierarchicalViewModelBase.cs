using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Extensibility {

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

        public static BitmapSource ApplyOverlay(BitmapSource image, string overlayUri) {
            BitmapSource overlay = GetImage(overlayUri);
            int height = (int) image.Height;
            int width = (int) image.Width;
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

    public abstract class HierarchicalViewModelBase {

        private bool _expanded;

        public bool IsSelected { get; set; }
        public bool IsChanged { get; set; }
        public abstract string Label { get; }
        public abstract BitmapSource Icon { get; }

        public ObservableCollection<HierarchicalViewModelBase> Children { get; set; }

        public bool IsExpanded {
            get { return _expanded; }
            set {
                if (value == true && !IsChildrenLoaded) {
                    if (LazyLoadChildren != null) {
                        LazyLoadChildren(this);
                    }
                }
                _expanded = value;
            }
        }

        public bool IsChildrenLoaded {
            get {
                if (Children == null) {
                    return false;
                }

                if (Children.Count == 1 && Children[0] is ViewModelPlaceholder) {
                    return false;
                }

                return true;
            }
        }

        public event ViewModelExpandedDelegate LazyLoadChildren;

    }

    public delegate void ViewModelExpandedDelegate(HierarchicalViewModelBase item);


    public class ViewModelPlaceholder : HierarchicalViewModelBase {

        private string _label;

        public ViewModelPlaceholder(string label) {
            _label = label;
        }

        public override string Label {
            get { return _label; }
        }


        public override BitmapSource Icon {
            get {
                return null;
            }
        }
    }
}
