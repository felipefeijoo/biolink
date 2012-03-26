using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using SharpMap.Layers;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;

namespace BioLink.Client.Maps {

    public class MapLegend {

        private const int RESIZE_AREA_HEIGHT = 16;
        private const int RESIZE_AREA_WIDTH = 16;

        private bool _hasCursor = false;
        private Point? _downPoint;

        private Image _sizeHandle;

        public MapLegend(MapBox map) {
            this.MapBox = map;
            IsVisible = true;

            Top = 50;
            Left = 50;
            Height = 80;
            Width = 150;

            var sizer = ImageCache.GetImage("pack://application:,,,/BioLink.Client.Maps;component/images/SizeHandle.png");
            _sizeHandle = GraphicsUtils.ImageFromBitmapSource(sizer);

            NumberOfColumns = 2;

            Title = "Legend";
            TitleFont = new Font("Tahoma", 12, FontStyle.Regular);
            TitleBrush = SystemBrushes.WindowText;

            ItemFont = new Font("Tahoma", 10, FontStyle.Regular);
            ItemBrush = SystemBrushes.ControlText;

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

                    g.SetClip(new Rectangle(Left, Top, Width, Height));

                    g.FillRectangle(BackgroundBrush, Left, Top, Width, Height);

                    if (!String.IsNullOrEmpty(_debugMessage)) {
                        g.DrawString(_debugMessage, MapBox.Font, Brushes.Red, 10, MapBox.Height - 50);
                    }

                    g.DrawRectangle(BorderPen, Left, Top, Width, Height);

                    g.DrawImage(_sizeHandle, Left + (Width - _sizeHandle.Width - BorderPen.Width), Top + (Height - _sizeHandle.Height - BorderPen.Width));

                    var titleSize = g.MeasureString(Title, TitleFont);

                    var titlePoint = new Point((int) (Left + (Width / 2) - (titleSize.Width / 2)), Top + 5);

                    g.DrawString(Title, TitleFont, TitleBrush, titlePoint);

                    int col = 0;
                    int row = 0;

                    foreach (ILayer layer in MapBox.Map.Layers) {
                        DrawLayerItem(g, layer, row, col);
                        col++;
                        if (col > NumberOfColumns) {
                            col = 0;
                            row++;
                        }
                    }
                    
                } catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                } finally {
                    g.Dispose();
                }
            }
        }

        private void DrawLayerItem(Graphics g, ILayer layer, int row, int col) {
            
            var titleSize = g.MeasureString(Title, TitleFont);
            int rowHeight = (int) ItemFont.GetHeight() + 6;
            int colWidth = Width / NumberOfColumns;
            int top = (int) (titleSize.Height + 10) + (rowHeight * row) + Top;
            int left = col * colWidth + Left;

            Image icon = null;

            if (layer is VectorLayer) {
                var vectorLayer = layer as VectorLayer;
                icon = vectorLayer.Style.Symbol;
                if (icon == null) {
                    icon = new Bitmap(12, 12);
                    var gg = Graphics.FromImage(icon);
                    gg.FillRectangle(vectorLayer.Style.Fill, 0, 0, 11, 11);
                    gg.DrawRectangle(Pens.Black, 0, 0, 11, 11);
                }
            } 

            var iconWidth = icon != null ? icon.Width : 12;

            var labelRect = new RectangleF(left + iconWidth + 10 , top, colWidth - (iconWidth + 6), rowHeight);

            if (icon != null) {
                var iconLeft = left + 5;
                var iconTop = (top + rowHeight / 2) - (icon.Height / 2);
                g.DrawImage(icon, iconLeft, iconTop);
            }

            var format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            g.DrawString(layer.LayerName, ItemFont, ItemBrush, labelRect, format);
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

        public Font ItemFont { get; set; }
        public Brush ItemBrush { get; set; }

        public int NumberOfColumns { get; set; }

    }
}
