using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.IO;

namespace BioLink.Client.Utilities {
    public static class GraphicsUtils {

        public static BitmapSource SystemDrawingImageToBitmapSource(System.Drawing.Image image) {
            using (System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image)) {
                IntPtr hBitmap = bitmap.GetHbitmap();
                System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                SystemUtils.DeleteObject(hBitmap);
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

        public static BitmapSource ExtractIconForExtension(string ext) {
            if (ext != null) {
                System.Drawing.Icon icon = SystemUtils.GetIconFromExtension(ext);
                if (icon != null) {
                    return SystemDrawingIconToBitmapSource(icon);
                }
            }
            return null;
        }

        public static BitmapFrame Resize(BitmapFrame photo, int width, int height, BitmapScalingMode scalingMode) {
            var group = new DrawingGroup();

            RenderOptions.SetBitmapScalingMode(group, scalingMode);
            group.Children.Add(new ImageDrawing(photo, new System.Windows.Rect(0, 0, width, height)));
            var targetVisual = new DrawingVisual();
            var targetContext = targetVisual.RenderOpen();
            targetContext.DrawDrawing(group);
            var target = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            targetContext.Close();
            target.Render(targetVisual);
            var targetFrame = BitmapFrame.Create(target);
            return targetFrame;
        }


        public static System.Windows.Media.Imaging.BitmapSource SystemDrawingIconToBitmapSource(Icon icon) {
            //Create bitmap
            var bmp = icon.ToBitmap();
            return SystemDrawingBitmapToBitmapSource(bmp);
        }


        public static System.Windows.Media.Imaging.BitmapSource SystemDrawingBitmapToBitmapSource(System.Drawing.Bitmap bitmap) {

            // allocate the memory for the bitmap            
            IntPtr bmpPt = bitmap.GetHbitmap();

            // create the bitmapSource
            System.Windows.Media.Imaging.BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmpPt,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

            // freeze the bitmap to avoid hooking events to the bitmap
            bitmapSource.Freeze();

            // free memory
            SystemUtils.DeleteObject(bmpPt);

            return bitmapSource;
        }

        public static BitmapSource LoadImageFromFile(string filename) {
            if (!String.IsNullOrEmpty(filename)) {
                try {
                    using (var fs = new FileStream(filename, FileMode.Open)) {
                        var imageDecoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        var image = imageDecoder.Frames[0];
                        return image;
                    }
                } catch (Exception) {
                    FileInfo finfo = new FileInfo(filename);
                    return GraphicsUtils.ExtractIconForExtension(finfo.Extension.Substring(1));
                }
            }

            return null;

        }

        public static BitmapSource GenerateThumbnail(string filename, int maxDimension) {
            if (!String.IsNullOrEmpty(filename)) {
                try {
                    using (var fs = new FileStream(filename, FileMode.Open)) {
                        var imageDecoder = BitmapDecoder.Create(fs, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        var image = imageDecoder.Frames[0];

                        int height = maxDimension;
                        int width = maxDimension;

                        if (image.Height > image.Width) {
                            width = (int)(image.Width * ((double)maxDimension / image.Height));
                        } else {
                            height = (int)(image.Height * ((double)maxDimension / image.Width));
                        }

                        return GraphicsUtils.Resize(image, width, height, BitmapScalingMode.HighQuality);
                    }
                } catch (Exception) {
                    FileInfo finfo = new FileInfo(filename);
                    return GraphicsUtils.ExtractIconForExtension(finfo.Extension.Substring(1));
                }
            }

            return null;
        }



    }
}
