/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.IO;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Utility functions for manipulating images
    /// </summary>
    public static class GraphicsUtils {

        /// <summary>
        /// This map is a cache of filename extension to icon
        /// </summary>
        private static Dictionary<string, BitmapSource> _ExtensionIconMap = new Dictionary<string, BitmapSource>();

        /// <summary>
        /// Converts a (legacy) System Drawing Image to a WPF bitmap source
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Hatch styles are used to fill map regions
        /// </summary>
        /// <param name="brush"></param>
        /// <returns></returns>
        public static System.Drawing.Drawing2D.HatchStyle? GetHatchStyleFromBrush(System.Drawing.Brush brush) {
            if (brush is System.Drawing.Drawing2D.HatchBrush) {
                return (brush as System.Drawing.Drawing2D.HatchBrush).HatchStyle;
            }
            return null;
        }

        /// <summary>
        /// Converts a brush to a color
        /// </summary>
        /// <param name="brush"></param>
        /// <returns></returns>
        public static System.Drawing.Color GetColorFromBrush(System.Drawing.Brush brush) {

            if (brush is System.Drawing.SolidBrush) {
                return (brush as System.Drawing.SolidBrush).Color;
            }

            if (brush is System.Drawing.Drawing2D.HatchBrush) {
                return (brush as System.Drawing.Drawing2D.HatchBrush).ForegroundColor;
            }

            throw new Exception("Unhandled brush type! Could not extract brush color");
        }

        /// <summary>
        /// Constructs a System Drawing brush
        /// </summary>
        /// <param name="color"></param>
        /// <param name="hatchStyle"></param>
        /// <returns></returns>
        public static System.Drawing.Brush CreateBrush(System.Drawing.Color color, System.Drawing.Drawing2D.HatchStyle? hatchStyle) {
            if (hatchStyle != null && hatchStyle.HasValue) {
                return new System.Drawing.Drawing2D.HatchBrush(hatchStyle.Value, color, System.Drawing.Color.FromArgb(0, 0, 0, 0));
            } else {
                return new System.Drawing.SolidBrush(color);
            }
        }

        /// <summary>
        /// Returns the icon that represents a given file extension (as an ImageSource). Will cache for each extension
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static BitmapSource ExtractIconForExtension(string ext) {
            if (ext != null) {
                lock (_ExtensionIconMap) {
                    var key = ext.ToUpper();
                    if (_ExtensionIconMap.ContainsKey(key)) {
                        return _ExtensionIconMap[key];
                    }

                    System.Drawing.Icon icon = SystemUtils.GetIconFromExtension(ext);
                    if (icon != null) {                    
                        var bitmap = SystemDrawingIconToBitmapSource(icon);
                        _ExtensionIconMap[key] = bitmap;
                        return bitmap;
                    } 
                }
            }
            return null;
        }

        /// <summary>
        /// Resizes a bitmap frame
        /// </summary>
        /// <param name="photo"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="scalingMode"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Converts an Icon to a BitmapSource
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static System.Windows.Media.Imaging.BitmapSource SystemDrawingIconToBitmapSource(Icon icon) {
            //Create bitmap
            var bmp = icon.ToBitmap();
            return SystemDrawingBitmapToBitmapSource(bmp);
        }

        /// <summary>
        /// Converts between System Drawing bitmaps to WPF BitmapSources
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Attempts to load an image from file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static BitmapSource LoadImageFromFile(string filename) {
            if (!String.IsNullOrEmpty(filename)) {
                try {
                    using (var fs = new FileStream(filename, FileMode.Open)) {
                        var imageDecoder = BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                        var image = imageDecoder.Frames[0];
                        return image;
                    }
                } catch (Exception) {
                    return GetIconForFilePath(filename);
                }
            }

            return null;

        }

        /// <summary>
        /// Attempts to extract a representative icon for a file path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BitmapSource GetIconForFilePath(string path) {
            
            try {
                var result = Icon.ExtractAssociatedIcon(path);
                if (result != null) {
                    return SystemDrawingIconToBitmapSource(result);
                }
            } catch (Exception) {
            }

            return null;
        }

        /// <summary>
        /// Generates a thumbnail for a file. If the file is an image file, a proper thumbnail is created, otherwise an icon based on the files extension is produced
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="maxDimension"></param>
        /// <returns></returns>
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
                    var result = GraphicsUtils.ExtractIconForExtension(finfo.Extension.Substring(1));
                    if (result == null) {
                        result = GraphicsUtils.GetIconForFilePath(filename);
                    }
                    return result;
                }
            }

            return null;
        }

    }
}
