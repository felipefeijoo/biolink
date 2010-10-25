using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpMap.Layers;
using SharpMap.Data.Providers;

namespace BioLink.Client.Maps {

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

}
