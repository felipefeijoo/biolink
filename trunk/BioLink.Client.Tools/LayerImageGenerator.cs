using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BioLink.Client.Tools {
    public class LayerImageGenerator {

        //public ImageSource GetImageForLayer(GridLayer layer, Color lowcolor, Color highcolor, Color novaluecolor, int intervals = 256) {
        //    //var palette = CreateGradientPalette(lowcolor, highcolor, novaluecolor, intervals);
        //    //var bmp = new WriteableBitmap(layer.Width, layer.Height, 96, 96, PixelFormats.Rgb24)
        //    //return null;
        //}

        public Color[] CreateGradientPalette(Color lowcolor, Color highcolor, Color noValueColor, int intervals = 256) {
            var palette = new Color[intervals];
            var r1 = lowcolor.R;
            var g1 = lowcolor.G;
            var b1 = lowcolor.B;
            var r2 = highcolor.R;
            var g2 = highcolor.G;
            var b2 = highcolor.B;

            float deltaR = ((float)(r2 - r1)) / ((float)(intervals - 1));
            float deltaG = ((float)(g2 - g1)) / ((float)(intervals - 1));
            float deltaB = ((float)(b2 - b1)) / ((float)(intervals - 1));

            float r = r1;
            float g = g1;
            float b = b1;


            for (int i = 1; i <= intervals && i <= 255; ++i) {
                palette[i] = Color.FromRgb((byte) r, (byte) g, (byte) b);
                r += deltaR;
                g += deltaG;
                b += deltaB;
            }

            palette[0] = noValueColor;

            return palette;                
        }
    }
}
