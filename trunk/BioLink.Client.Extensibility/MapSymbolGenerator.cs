using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace BioLink.Client.Extensibility {

    public class MapSymbolGenerator {

        public static Image GetSymbolForPointSet(MapPointSet set) {
            return GetSymbol(set.PointShape, set.Size, set.PointColor, set.DrawOutline, set.OutlineColor);
        }

        public static Image GetSymbol(PointOptionsControl ctl) {
            return GetSymbol(ctl.Shape, ctl.Size, ctl.Color, ctl.DrawOutline);
        }

        public static Image GetSymbol(MapPointShape shape, int size, System.Windows.Media.Color fill, bool drawOutline, System.Windows.Media.Color outline = default(System.Windows.Media.Color)) {
            Color fillColor = Color.FromArgb(fill.A, fill.R, fill.G, fill.B);

            if (outline == default(System.Windows.Media.Color)) {
                outline = System.Windows.Media.Colors.Black;
            }

            Color outlineColor = Color.FromArgb(outline.A, outline.R, outline.G, outline.B);

            Image img = null;

            switch (shape) {
                case MapPointShape.Circle:
                    img = Circle(size, fillColor, drawOutline, outlineColor);
                    break;
                case MapPointShape.Square:
                    img = Square(size, fillColor, drawOutline, outlineColor);
                    break;
                case MapPointShape.Triangle:
                    img = Triangle(size, fillColor, drawOutline, outlineColor);
                    break;
            }

            if (img != null) {
                var info = new SymbolInfo { Size = size, Color = fillColor, DrawOutline = drawOutline, Shape = shape };
                img.Tag = info;
            }

            return img;
        }

        public static Image Circle(int size, Color fillColor, bool drawOutline, Color outlineColor = default(Color)) {
            Bitmap bm = new Bitmap(size, size);
            
            using (Graphics g = Graphics.FromImage(bm)) {
                Brush fill = new SolidBrush(fillColor);
                g.FillEllipse(fill, new Rectangle(0, 0, size - 1, size - 1));
                if (drawOutline) {
                    Pen outline = new Pen(outlineColor);
                    g.DrawEllipse(outline, new Rectangle(0, 0, size - 1, size - 1));
                }
            }
            return bm;
        }

        public static Image Square(int size, Color fillColor, bool drawOutline, Color outlineColor = default(Color)) {
            Bitmap bm = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(bm)) {
                Brush fill = new SolidBrush(fillColor);
                g.FillRectangle(fill, new Rectangle(0, 0, size - 1, size - 1));
                if (drawOutline) {
                    Pen outline = new Pen(outlineColor);
                    g.DrawRectangle(outline, new Rectangle(0, 0, size - 1, size - 1));
                }
            }
            return bm;
        }

        public static Image Triangle(int size, Color fillColor, bool drawOutline, Color outlineColor = default(Color)) {
            Bitmap bm = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(bm)) {
                Brush fill = new SolidBrush(fillColor);
                // new Point[] { new Point(0, size-1), new Point(size/2, 0), new Point(size-1, size-1) }, new byte[] { PathPointType.Start, PathPointType.Line, PathPointType.Line}
                GraphicsPath path = new GraphicsPath();

                path.AddLine(0, size - 1, (size - 1) / 2, 0);
                path.AddLine((size - 1) / 2, 0, size - 1, size - 1);
                path.CloseFigure();

                g.FillPath(fill, path);                
                if (drawOutline) {
                    Pen outline = new Pen(outlineColor);
                    g.DrawPath(outline, path);
                }
            }            
            return bm;
        }
    }

    public class SymbolInfo {
        public int Size { get; set; }
        public MapPointShape Shape { get; set; }
        public Color Color { get; set; }
        public bool DrawOutline { get; set; }
    }
    
}
