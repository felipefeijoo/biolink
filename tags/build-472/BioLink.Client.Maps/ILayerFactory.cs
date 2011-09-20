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
using SharpMap.Layers;
using SharpMap.Data.Providers;
using BioLink.Client.Utilities;

namespace BioLink.Client.Maps {

    public static class LayerFileLoader {

        private static Dictionary<string, ILayerFactory> _layerFactoryCatalog = new Dictionary<string, ILayerFactory>();

        static LayerFileLoader() {
            _layerFactoryCatalog[".shp"] = new ShapeFileLayerFactory();
            // _layerFactoryCatalog[".png"] = new RasterFileLayerFactory();
            _layerFactoryCatalog[".bmp"] = new RasterFileLayerFactory();
        }

        public static ILayer LoadLayer(string filename) {

            Logger.Debug("Attempting to load Map Layer from file '{0}'", filename);

            string extension = System.IO.Path.GetExtension(filename);
            ILayerFactory layerFactory = null;

            if (!_layerFactoryCatalog.TryGetValue(extension.ToLower(), out layerFactory)) {
                Logger.Debug("No appropriate layer factory could be found for file '{0}'", filename);
                return null;
            }

            ILayer layer = layerFactory.Create(System.IO.Path.GetFileNameWithoutExtension(filename), filename);
            return layer;

        }

    }

    public interface ILayerFactory {
        ILayer Create(string layerName, string connectionInfo);
    }

    public class ShapeFileLayerFactory : ILayerFactory {

        public ILayer Create(string layerName, string connectionInfo) {
            ShapeFile shapeFileData = new ShapeFile(connectionInfo);
            VectorLayer shapeFileLayer = new VectorLayer(layerName, shapeFileData);
            shapeFileLayer.Style.Fill = new System.Drawing.SolidBrush(System.Drawing.Color.Wheat);
            shapeFileLayer.Style.Outline = new System.Drawing.Pen(new System.Drawing.SolidBrush(System.Drawing.Color.Black), 1);
            shapeFileLayer.Style.Enabled = true;
            shapeFileLayer.Style.EnableOutline = true;

            return shapeFileLayer;
        }

    }

    public class RasterFileLayerFactory : ILayerFactory {

        public ILayer Create(string layerName, string connectionInfo) {
            return new MyGdalRasterLayer(layerName, connectionInfo);
        }
    }

}
