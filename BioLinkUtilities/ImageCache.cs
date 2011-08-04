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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Utilities {

    /// <summary>
    /// Utility class that performs two functions:
    /// <list type="bullet">
    /// <item>Simplifies access to images embedded as resources within assemblies</item>
    /// <item>Caches images for performance and efficiency</item>
    /// </list>
    /// </summary>
    public static class ImageCache {

        /* Image cache */
        private static Dictionary<String, BitmapSource> _cache = new Dictionary<string, BitmapSource>();

        /// <summary>
        /// Get an image by URI (may be a packed resource URI)
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Helper for retrieve images packed into an assembly
        /// </summary>
        /// <param name="localpath"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static BitmapSource GetPackedImage(string localpath, string assemblyName) {
            string uri = String.Format("pack://application:,,,/{0};component/{1}", assemblyName, localpath);
            return GetImage(uri);
        }

        /// <summary>
        /// Retrieves that image at overlayUri, and overlays it on imgSource and returns the resulting image
        /// </summary>
        /// <param name="imageSrc"></param>
        /// <param name="overlayUri"></param>
        /// <returns></returns>
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
