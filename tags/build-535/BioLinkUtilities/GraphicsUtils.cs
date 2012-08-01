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
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Controls;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Utility functions for manipulating images
    /// </summary>
    public static class GraphicsUtils {

        /// <summary>
        /// This map is a cache of filename extension to icon
        /// </summary>
        private static readonly Dictionary<string, BitmapSource> ExtensionIconMap = new Dictionary<string, BitmapSource>();

        /// <summary>
        /// Converts a (legacy) System Drawing Image to a WPF bitmap source
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static BitmapSource SystemDrawingImageToBitmapSource(System.Drawing.Image image) {
            using (var bitmap = new Bitmap(image)) {
                IntPtr hBitmap = bitmap.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
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

            if (brush is SolidBrush) {
                return (brush as SolidBrush).Color;
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
            if (hatchStyle != null) {
                return new System.Drawing.Drawing2D.HatchBrush(hatchStyle.Value, color, System.Drawing.Color.FromArgb(0, 0, 0, 0));
            }
            return new SolidBrush(color);
        }

        /// <summary>
        /// Returns the icon that represents a given file extension (as an ImageSource). Will cache for each extension
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static BitmapSource ExtractIconForExtension(string ext) {
            if (ext != null) {
                lock (ExtensionIconMap) {
                    var key = ext.ToUpper();
                    if (ExtensionIconMap.ContainsKey(key)) {
                        return ExtensionIconMap[key];
                    }

                    Icon icon = SystemUtils.GetIconFromExtension(ext);
                    if (icon != null) {                    
                        var bitmap = SystemDrawingIconToBitmapSource(icon);
                        ExtensionIconMap[key] = bitmap;
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
        public static BitmapFrame Resize(BitmapFrame photo, int width, int height, System.Windows.Media.BitmapScalingMode scalingMode) {
            var group = new System.Windows.Media.DrawingGroup();

            System.Windows.Media.RenderOptions.SetBitmapScalingMode(group, scalingMode);
            group.Children.Add(new System.Windows.Media.ImageDrawing(photo, new System.Windows.Rect(0, 0, width, height)));
            var targetVisual = new System.Windows.Media.DrawingVisual();
            var targetContext = targetVisual.RenderOpen();
            targetContext.DrawDrawing(group);
            var target = new RenderTargetBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Default);
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
        public static BitmapSource SystemDrawingIconToBitmapSource(Icon icon) {
            //Create bitmap
            var bmp = icon.ToBitmap();
            return SystemDrawingBitmapToBitmapSource(bmp);
        }

        /// <summary>
        /// Converts between System Drawing bitmaps to WPF BitmapSources
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static BitmapSource SystemDrawingBitmapToBitmapSource(Bitmap bitmap) {

            // allocate the memory for the bitmap            
            IntPtr bmpPt = bitmap.GetHbitmap();

            // create the bitmapSource
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bmpPt,
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            // freeze the bitmap to avoid hooking events to the bitmap
            bitmapSource.Freeze();

            // free memory
            SystemUtils.DeleteObject(bmpPt);

            return bitmapSource;
        }

        /// <summary>
        /// Converts a WPF bitmap source to a System.Drawing image (bitmap)
        /// </summary>
        /// <param name="bitmapsource"></param>
        /// <returns></returns>
        public static System.Drawing.Image ImageFromBitmapSource(BitmapSource source) {
            //System.Drawing.Bitmap bitmap;
            //using (MemoryStream outStream = new MemoryStream()) {
            //    BitmapEncoder enc = new BmpBitmapEncoder();
            //    enc.Frames.Add(BitmapFrame.Create(source));
            //    enc.Save(outStream);
            //    bitmap = new System.Drawing.Bitmap(outStream);
            //}
            //return bitmap;
            Bitmap bmp = new Bitmap(source.PixelWidth, source.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
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
                var icon = Icon.ExtractAssociatedIcon(path);
                if (icon != null) {
                    return SystemDrawingIconToBitmapSource(icon);
                }
            } catch (Exception) {
                return null;
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
                            width = (int)(image.Width * (maxDimension / image.Height));
                        } else {
                            height = (int)(image.Height * (maxDimension / image.Width));
                        }

                        return Resize(image, width, height, System.Windows.Media.BitmapScalingMode.HighQuality);
                    }
                } catch (Exception) {
                    var finfo = new FileInfo(filename);
                    return ExtractIconForExtension(finfo.Extension.Substring(1)) ?? GetIconForFilePath(filename);                    
                }
            }

            return null;
        }

        public static void ApplyLegacyFont(System.Windows.FrameworkElement element, Font font) {
            var style = System.Windows.FontStyles.Normal;            
            if (font.Italic) {
                style = System.Windows.FontStyles.Italic;
            }
            var weight = System.Windows.FontWeights.Normal;
            if (font.Bold) {
                weight = System.Windows.FontWeights.Bold;
            }

            float size = (float) ((font.Size *  96.0) / 72.0);
            var family = new System.Windows.Media.FontFamily(font.FontFamily.Name);

            if (element is Control) {
                var control = element as Control;
                control.FontFamily = family;
                control.FontSize = size;
                control.FontStyle = style;
                control.FontWeight = weight;
            }

            if (element is TextBlock) {
                var textBlock = element as TextBlock;
                textBlock.FontFamily = family;
                textBlock.FontSize = size;
                textBlock.FontStyle = style;
                textBlock.FontWeight = weight;

                if (font.Underline) {
                    textBlock.TextDecorations.Add(System.Windows.TextDecorations.Underline);
                }

                if (font.Strikeout) {
                    textBlock.TextDecorations.Add(System.Windows.TextDecorations.Strikethrough);
                }

            }
            
        }

        public static Font ScaleFont(Font font, System.Drawing.Image image) {
            //float size = (font.Size * 72) / image.HorizontalResolution;
            //return new Font(font.FontFamily, size, font.Style);
            return font;
        }

        public static Font GetLegacyFont(TextBlock element) {

            System.Drawing.FontStyle style = 0;

            FontStyle[] s = new System.Drawing.FontStyle[] { FontStyle.Regular, FontStyle.Bold, FontStyle.Italic, FontStyle.Strikeout, FontStyle.Underline};

            // Find the first default style that will work with this font...
            var family = new System.Drawing.FontFamily(element.FontFamily.Source);
            foreach (FontStyle fs in s) {
                if (family.IsStyleAvailable(fs)) {
                    style = fs;
                    break;
                }
            }

            // Scale the font from 96 DPI to 72 DPI 
            float size = (float)((element.FontSize * 72.0) / 96.0);

            if (element.FontWeight == System.Windows.FontWeights.Bold) {
                style |= System.Drawing.FontStyle.Bold;
            }

            if (element.FontStyle == System.Windows.FontStyles.Italic || element.FontStyle == System.Windows.FontStyles.Oblique) {
                style |= System.Drawing.FontStyle.Italic;
            }

            if (element.TextDecorations.Contains(System.Windows.TextDecorations.Underline[0])) {
                style |= System.Drawing.FontStyle.Underline;
            }

            if (element.TextDecorations.Contains(System.Windows.TextDecorations.Strikethrough[0])) {
                style |= System.Drawing.FontStyle.Strikeout;
            }
            
            var font = new Font(family, size, style);

            return font;
        }

    }
}
