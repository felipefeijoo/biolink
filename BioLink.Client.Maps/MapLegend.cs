using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using SharpMap.Layers;
using BioLink.Client.Extensibility;
using BioLink.Client.Utilities;
using BioLink.Data;
using System.Drawing.Text;

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

            TitleColor = Color.Black;

            MapBox.MouseMove += new Maps.MapBox.MouseEventHandler(MapBox_MouseMove);
            MapBox.MouseDown += new Maps.MapBox.MouseEventHandler(MapBox_MouseDown);

            ReadFromSettings();
            
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
            return IsMouseInResizeBox(ImagePos.Location);
        }

        private bool IsMouseInResizeBox(Point p) {
            return (p.X >= (Left + Width - RESIZE_AREA_WIDTH - BorderWidth) && p.X <= Left + Width) && (p.Y >= (Top + Height - RESIZE_AREA_HEIGHT - BorderWidth) && p.Y <= Top + Height);
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

        public void Draw(Image destination) {

            if (IsVisible) {
                var titleFont = GraphicsUtils.ScaleFont(TitleFont, destination);
                var g = Graphics.FromImage(destination);
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                try {                    
                    g.SetClip(new Rectangle(Left - BorderWidth, Top - BorderWidth, Width + (BorderWidth * 2), Height + (BorderWidth * 2)));

                    g.FillRectangle(new SolidBrush(BackgroundColor), Left, Top, Width, Height);

                    if (BorderWidth > 0) {
                        var borderPen = new Pen(BorderColor, BorderWidth);
                        borderPen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;

                        g.DrawRectangle(borderPen, Left, Top, Width, Height);
                    }

                    g.DrawImage(_sizeHandle, Left + (Width - _sizeHandle.Width - BorderWidth), Top + (Height - _sizeHandle.Height - BorderWidth));

                    var titleSize = g.MeasureString(Title, titleFont);
                    var titlePoint = new Point((int) (Left + (Width / 2) - (titleSize.Width / 2)), Top + 5 + BorderWidth);

                    g.DrawString(Title, titleFont, new SolidBrush(TitleColor), titlePoint);

                    int col = 0;
                    int row = 0;

                    var layerItems = GetLayerDescriptors();

                    foreach (ILayer layer in MapBox.Map.Layers) {
                        var desc = layerItems[layer.LayerName];
                        if (desc.IsVisible) {
                            DrawLayerItem(g, layer, row, col, desc, titleSize, destination);
                            col++;
                            if (col >= NumberOfColumns) {
                                col = 0;
                                row++;
                            }
                        }
                    }
                    
                } catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                } finally {
                    g.Dispose();
                }
            }
        }

        private void DrawLayerItem(Graphics g, ILayer layer, int row, int col,  LegendItemDescriptor desc, SizeF titleSize, Image dest) {

            var itemFont = GraphicsUtils.ScaleFont(ItemFont, dest);

            int rowHeight = (int) itemFont.GetHeight() + 6;
            int colWidth = Width / NumberOfColumns;
            int top = (int) (titleSize.Height + 10 + BorderWidth) + (rowHeight * row) + Top;
            int left = col * colWidth + Left + BorderWidth;

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

            var labelRect = new RectangleF(left + iconWidth + 10, top, colWidth - (iconWidth + 10), rowHeight);

            if (icon != null) {
                var iconLeft = left + 5;
                var iconTop = (top + rowHeight / 2) - (icon.Height / 2);
                g.DrawImage(icon, iconLeft, iconTop);
            }

            var format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            g.DrawString(desc.Title, itemFont, new SolidBrush(desc.TitleColor), labelRect, format);
        }

        public MapBox MapBox { get; private set; }

        public bool IsVisible { get; set; }

        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Color BorderColor { get; set; }
        public int BorderWidth { get; set; }

        public Color BackgroundColor { get; set; }

        public String Title { get; set; }
        public Font TitleFont { get; set; }
        public Color TitleColor { get; set; }

        public Font ItemFont { get; set; }

        public int NumberOfColumns { get; set; }

        private User User { 
            get { return PluginManager.Instance.User; }
        }

        private Dictionary<String, LegendItemDescriptor> GetLayerDescriptors() {
            var items = Config.GetUser(User, "MapLegend.LayerDescriptors", new Dictionary<String, LegendItemDescriptor>());
            var found = new List<String>();
            foreach (ILayer layer in MapBox.Map.Layers) {
                found.Add(layer.LayerName);
                if (!items.ContainsKey(layer.LayerName)) {
                    items.Add(layer.LayerName, new LegendItemDescriptor { LayerName = layer.LayerName, IsVisible = true, Title = layer.LayerName, TitleColor = Color.Black });
                }
            }

            var killList = new List<String>();
            foreach (String name in items.Keys) {
                if (!found.Contains(name)) {
                    killList.Add(name);
                }
            }

            foreach (String name in killList) {
                items.Remove(name);
            }

            return items;
        }

        public void ReadFromSettings() {
            BackgroundColor = Config.GetUser(User, "MapLegend.BackColor", Color.White);
            BorderColor = Config.GetUser(User, "MapLegend.BorderColor", Color.Black);
            BorderWidth = Config.GetUser(User, "MapLegend.BorderWidth", 1);
            Title = Config.GetUser(User, "MapLegend.Title", "Legend");
            TitleFont = Config.GetUser(User, "MapLegend.TitleFont", new Font("Tahoma", 12));
            NumberOfColumns = Config.GetUser(User, "MapLegend.NumberOfColumns", 1);
            ItemFont = Config.GetUser(User, "MapLegend.ItemFont", new Font("Tahoma", 8));
        }

        public void SaveToSettings() {
            Config.SetUser(User, "MapLegend.BackColor", BackgroundColor);
            Config.SetUser(User, "MapLegend.BorderColor", BorderColor);
            Config.SetUser(User, "MapLegend.BorderWidth", BorderWidth);
            Config.SetUser(User, "MapLegend.Title", Title);
            Config.SetUser(User, "MapLegend.TitleFont", TitleFont);
            Config.SetUser(User, "MapLegend.NumberOfColumns", NumberOfColumns);
            Config.SetUser(User, "MapLegend.ItemFont", ItemFont);
            Config.SetUser(User, "MapLegend.LayerDescriptors", GetLayerDescriptors());            
        }

        public Rectangle Position {
            get {
                return new Rectangle(Left, Top, Width, Height);
            }
            set {
                Left = value.X;
                Top = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }

    }

    public class LegendItemDescriptor {
        public String LayerName { get; set; }
        public bool IsVisible { get; set; }
        public String Title { get; set; }        
        public Color TitleColor { get; set; }
    }
}
