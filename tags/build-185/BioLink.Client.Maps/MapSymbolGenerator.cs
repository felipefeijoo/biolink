using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using BioLink.Client.Extensibility;

namespace BioLink.Client.Maps {

    public class MapSymbolGenerator {

        public static Image GetSymbolForPointSet(MapPointSet set) {
            Color fillColor = Color.FromArgb(set.PointColor.A, set.PointColor.R, set.PointColor.G, set.PointColor.B);
            Color outlineColor = Color.FromArgb(set.OutlineColor.A, set.OutlineColor.R, set.OutlineColor.G, set.OutlineColor.B);
            switch (set.PointShape) {
                case MapPointShape.Circle:
                    return Circle(set.Size, fillColor, set.DrawOutline, outlineColor);
                case MapPointShape.Square:
                    return Square(set.Size, fillColor, set.DrawOutline, outlineColor);
                case MapPointShape.Triangle:
                    return Triangle(set.Size, fillColor, set.DrawOutline, outlineColor);
            }

            return null;
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
}
