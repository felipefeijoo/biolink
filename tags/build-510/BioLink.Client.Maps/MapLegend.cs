using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace BioLink.Client.Maps {

    public class MapLegend {

        private const int RESIZE_AREA_HEIGHT = 10;
        private const int RESIZE_AREA_WIDTH = 10;

        private bool _hasCursor = false;
        private Point? _downPoint;

        public MapLegend(MapBox map) {
            this.MapBox = map;
            IsVisible = true;

            Top = 50;
            Left = 50;
            Height = 80;
            Width = 150;

            Title = "Legend";
            TitleFont = new Font("Arial", 14, FontStyle.Regular);
            TitleBrush = SystemBrushes.WindowText;

            BorderPen = new Pen(Color.Black, 2);
            BackgroundBrush = new SolidBrush(Color.FromArgb(200, Color.White));

            MapBox.MouseMove += new Maps.MapBox.MouseEventHandler(MapBox_MouseMove);
            MapBox.MouseDown += new Maps.MapBox.MouseEventHandler(MapBox_MouseDown);
            
        }

        void MapBox_MouseDown(SharpMap.Geometries.Point WorldPos, MouseEventArgs ImagePos) {
            if (IsMouseInLegendBox(ImagePos) && ImagePos.Button == MouseButtons.Left) {
                _downPoint = new Point(ImagePos.X, ImagePos.Y);
            } else {
                _downPoint = null;
            }
        }

        private bool IsMouseInLegendBox(System.Windows.Forms.MouseEventArgs ImagePos) {
            return (ImagePos.X >= Left && ImagePos.X <= Left + Width) && (ImagePos.Y >= Top && ImagePos.Y <= Top + Height);
        }

        private bool IsMouseInLegendBox(Point p) {
            return (p.X >= Left && p.X <= Left + Width) && (p.Y >= Top && p.Y <= Top + Height);
        }

        private bool IsMouseInResizeBox(MouseEventArgs ImagePos) {
            return (ImagePos.X >= (Left + Width - RESIZE_AREA_WIDTH) && ImagePos.X <= Left + Width) && (ImagePos.Y >= (Top + Height - RESIZE_AREA_HEIGHT) && ImagePos.Y <= Top + Height);
        }

        private bool IsMouseInResizeBox(Point p) {
            return (p.X >= (Left + Width - RESIZE_AREA_WIDTH) && p.X <= Left + Width) && (p.Y >= (Top + Height - RESIZE_AREA_HEIGHT) && p.Y <= Top + Height);
        }

        void MapBox_MouseMove(SharpMap.Geometries.Point WorldPos, System.Windows.Forms.MouseEventArgs ImagePos) {
            if (IsMouseInLegendBox(ImagePos)) {
                _hasCursor = true;
                if (IsMouseInResizeBox(ImagePos)) {
                    MapBox.Cursor = Cursors.SizeNWSE;
                } else {
                    MapBox.Cursor = Cursors.Hand;
                }
            } else {
                if (_hasCursor) {
                    MapBox.SetCursor();
                    _hasCursor = false;
                }
            }

            if (ImagePos.Button == MouseButtons.Left && _downPoint.HasValue) {
                var dx = _downPoint.Value.X - ImagePos.X;
                var dy = _downPoint.Value.Y - ImagePos.Y;

                if (IsMouseInResizeBox(_downPoint.Value)) {
                    Width -= dx;
                    Height -= dy;
                } else {
                    Top -= dy;
                    Left -= dx;
                }

                if (Top < 0) {
                    Top = 0;
                }

                if (Left < 0) {
                    Left = 0;
                }

                if (Left > MapBox.Width - Width) {
                    Left = MapBox.Width - Width;
                }

                if (Top > MapBox.Height - Height) {
                    Top = MapBox.Height - Height;
                }

                _downPoint = new Point(ImagePos.X, ImagePos.Y);

                MapBox.Refresh();
            }

        }

        private String _debugMessage = "";

        public void Draw(Image destination) {

            if (IsVisible) {
                var g = Graphics.FromImage(destination);
                try {
                    g.FillRectangle(BackgroundBrush, Left, Top, Width, Height);
                    g.DrawRectangle(BorderPen, Left, Top, Width, Height);

                    g.DrawLine(BorderPen, Left + Width - RESIZE_AREA_WIDTH, Top + Height, Left + Width, Top + Height - RESIZE_AREA_HEIGHT);

                    if (!String.IsNullOrEmpty(_debugMessage)) {
                        g.DrawString(_debugMessage, MapBox.Font, Brushes.Red, 10, MapBox.Height - 50);
                    }

                    var titleSize = g.MeasureString(Title, TitleFont);

                    var titlePoint = new Point((int) (Left + (Width / 2) - (titleSize.Width / 2)), Top + 5);

                    g.DrawString(Title, TitleFont, TitleBrush, titlePoint);
                    
                } finally {
                    g.Dispose();
                }
            }
        }

        public MapBox MapBox { get; private set; }

        public bool IsVisible { get; set; }

        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Pen BorderPen { get; set; }
        public Brush BackgroundBrush { get; set; }

        public String Title { get; set; }
        public Font TitleFont { get; set; }
        public Brush TitleBrush { get; set; }

    }
}
