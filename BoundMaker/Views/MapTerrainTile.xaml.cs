using BoundMaker.Models;
using BoundMaker.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BoundMaker.Views
{
    public partial class MapTerrainTile : UserControl
    {

        public static readonly SolidColorBrush TerrainColor = new SolidColorBrush(Color.FromRgb(153, 59, 13));
        public static readonly SolidColorBrush NullColor = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush TerrainLocationColor = new SolidColorBrush(Color.FromArgb(200, 186, 92, 46));
        public static readonly SolidColorBrush NullLocationColor = new SolidColorBrush(Color.FromArgb(200, 33, 33, 33));
        public static readonly BitmapImage CreepTile = new BitmapImage(new Uri("/Images/Tiles/creep.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage DirtTile = new BitmapImage(new Uri("/Images/Tiles/dirt.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage GrassTile = new BitmapImage(new Uri("/Images/Tiles/grass.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage NullTile = new BitmapImage(new Uri("/Images/Tiles/null.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage BasilicaTile = new BitmapImage(new Uri("/Images/Tiles/basilica.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage HighDirtTile = new BitmapImage(new Uri("/Images/Tiles/highdirt.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage HighGrassTile = new BitmapImage(new Uri("/Images/Tiles/highgrass.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage PanelsTile = new BitmapImage(new Uri("/Images/Tiles/panels.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage TempleTile = new BitmapImage(new Uri("/Images/Tiles/temple.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage TwilightDirtTile = new BitmapImage(new Uri("/Images/Tiles/twidirt.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage TwilightHighDirtTile = new BitmapImage(new Uri("/Images/Tiles/twihighdirt.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage TwilightRockTile = new BitmapImage(new Uri("/Images/Tiles/twirock.png", UriKind.RelativeOrAbsolute));
        public static readonly BitmapImage TwilightHighRockTile = new BitmapImage(new Uri("/Images/Tiles/twihighrock.png", UriKind.RelativeOrAbsolute));
        private static readonly Dictionary<string, BitmapImage> tileset = new Dictionary<string, BitmapImage>()
        {
            {"null", NullTile},
            {"dirt", DirtTile},
            {"grass", GrassTile},
            {"creep", CreepTile},
            {"basilica", BasilicaTile},
            {"highdirt", HighDirtTile},
            {"highgrass", HighGrassTile},
            {"panels", PanelsTile},
            {"temple", TempleTile},
            {"twidirt", TwilightDirtTile},
            {"twihighdirt", TwilightHighDirtTile},
            {"twirock", TwilightRockTile},
            {"twihighrock", TwilightHighRockTile},
        };

        private MainWindow mainWindow;

        public bool Highlighted { get; private set; }

        public MapTerrainTile()
        {
            InitializeComponent();
            TileImage.Source = NullTile;
            MouseEnter += new MouseEventHandler(MouseEnterEventHandler);
            MouseLeave += new MouseEventHandler(MouseLeaveEventHandler);
            MouseLeftButtonDown += new MouseButtonEventHandler(MouseLeftButtonDownEventHandler);
            MouseRightButtonDown += new MouseButtonEventHandler(MouseRightButtonDownEventHandler);
            MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUpEventHandler);
            MouseRightButtonUp += new MouseButtonEventHandler(MouseRightButtonUpEventHandler);
            MouseMove += new MouseEventHandler(MouseMoveEventHandler);
        }

        public void SetWindowInstance(MainWindow window)
        {
            mainWindow = window;
        }

        public void SetHighlight(bool shouldHighlight)
        {
            Opacity = shouldHighlight ? 0.8 : 1.0;
        }

        public void SetTerrain(string key)
        {
            try
            {
                TileImage.Source = tileset[key];
            }
            catch
            {
                TileImage.Source = NullTile;
            }
            GlobalState.ChangesMade = true;
            if (mainWindow != null)
            {
                mainWindow.RefreshTitle();
            }
        }

        public string Terrain => tileset.Keys.First(x => tileset[x] == TileImage.Source);

        public void LocationPreview()
        {
            Opacity = 0.7;
        }

        private void MouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            if (!GlobalState.DraggingLocation && !GlobalState.ResizingLocation)
            {
                if (GlobalState.Mode_Location && !GlobalState.Playing)
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        mainWindow.MapTerrain.RefreshGrid();
                        var temp = new Point(Grid.GetColumn(this), Grid.GetRow(this));
                        List<UIElement> tiles = LocationHighlightCells(mainWindow.MapTerrain, GlobalState.LocationStart, temp);
                        tiles.ShowLocation();
                    }
                    else if (e.RightButton != MouseButtonState.Pressed)
                    {
                        Highlighted = true;
                    }
                }
            }
        }

        private static List<UIElement> LocationHighlightCells(Grid grid, Point point1, Point point2)
        {
            var elements = new List<UIElement>();
            int column1 = (int)point1.X;
            int row1 = (int)point1.Y;
            int column2 = (int)point2.X;
            int row2 = (int)point2.Y;
            for (int row = 0; row <= row2 - row1; row++)
            {
                for (int col = 0; col <= column2 - column1; col++)
                {
                    elements.Add(grid.GetCell(row + row1, col + column1));
                }
                for (int col = 0; col >= column2 - column1; col--)
                {
                    elements.Add(grid.GetCell(row + row1, col + column1));
                }
            }
            for (int row = 0; row >= row2 - row1; row--)
            {
                for (int col = 0; col <= column2 - column1; col++)
                {
                    elements.Add(grid.GetCell(row + row1, col + column1));
                }
                for (int col = 0; col >= column2 - column1; col--)
                {
                    elements.Add(grid.GetCell(row + row1, col + column1));
                }
            }
            return elements;
        }

        private void MouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            Opacity = 1.0;
        }

        private void MouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (!GlobalState.DraggingLocation && !GlobalState.ResizingLocation)
            {
                if (GlobalState.Mode_Terrain)
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        foreach (UIElement tile in HighlightTerrainCells(mainWindow.MapTerrain, this, e))
                        {
                            ((MapTerrainTile)tile).SetTerrain(SelectedTileType());
                        }
                    }
                }
                else if (GlobalState.Mode_Location && !GlobalState.Playing)
                {
                    LocationPreview();
                    foreach (MapLocation m in GlobalState.Locations)
                    {
                        m.IsHitTestVisible = false;
                    }
                    GlobalState.LocationStart = new Point(Grid.GetColumn(this), Grid.GetRow(this));
                }
            }
        }

        private void MouseLeftButtonUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (!GlobalState.DraggingLocation && !GlobalState.ResizingLocation)
            {
                if (GlobalState.Mode_Terrain)
                {
                    SetHighlight(true);
                }
                else if (GlobalState.Mode_Location && !GlobalState.Playing)
                {
                    var temp = new Point(Grid.GetColumn(this), Grid.GetRow(this));
                    List<UIElement> tiles = LocationHighlightCells(mainWindow.MapTerrain, GlobalState.LocationStart, temp);
                    var m = new MapLocation(tiles.GetLocationLeft(), tiles.GetLocationTop(), tiles.GetLocationWidth(), tiles.GetLocationHeight());
                    mainWindow.MapCanvas.Children.Add(m);
                    m.SetWindowInstance(mainWindow);
                    GlobalState.Locations.Add(m);
                    mainWindow.Panel_Location.SetLocationNames();
                    foreach (MapLocation m1 in GlobalState.Locations)
                    {
                        m1.IsHitTestVisible = true;
                    }

                    GlobalState.ChangesMade = true;
                    mainWindow.RefreshTitle();
                    mainWindow.MapTerrain.RefreshGrid();
                }
            }
        }

        private void MouseRightButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {

            if (!GlobalState.DraggingLocation && !GlobalState.ResizingLocation)
            {
                if (GlobalState.Mode_Terrain)
                {
                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        foreach (UIElement tile in HighlightTerrainCells(mainWindow.MapTerrain, this, e))
                        {
                            ((MapTerrainTile)tile).SetTerrain("null");
                        }
                    }
                }
                else if (GlobalState.Mode_Location)
                {
                    Highlighted = false;
                }
            }
        }

        private void MouseRightButtonUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (!GlobalState.DraggingLocation)
            {
                SetHighlight(true);
            }
            GlobalState.ChangesMade = true;
            mainWindow.RefreshTitle();
        }

        private void MouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            if (GlobalState.Mode_Terrain)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    foreach (UIElement tile in HighlightTerrainCells(mainWindow.MapTerrain, this, e))
                    {
                        ((MapTerrainTile)tile).SetTerrain(SelectedTileType());
                    }
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    foreach (UIElement tile in HighlightTerrainCells(mainWindow.MapTerrain, this, e))
                    {
                        ((MapTerrainTile)tile).SetTerrain("null");
                    }
                }

                mainWindow.MapTerrain.RefreshGrid();
                IEnumerable<MapTerrainTile> tiles = HighlightTerrainCells(mainWindow.MapTerrain, this, e);
                tiles.HighlightAll();
            }
        }

        private static IEnumerable<MapTerrainTile> HighlightTerrainCells(Grid grid, UIElement tile, MouseEventArgs e)
        {
            int xStart;
            int yStart;
            var elements = new List<MapTerrainTile>();
            var p = new Point(Grid.GetRow(tile), Grid.GetColumn(tile));
            Point tp = e.GetPosition(tile);
            if (tp.Y <= Editor.GridSize / 2)
            {
                xStart = (int)p.X - (GlobalState.TerrainPlacementSize / 2);
            }
            else
            {
                xStart = (int)p.X - (GlobalState.TerrainPlacementSize / 3);
            }

            if (tp.X <= Editor.GridSize / 2)
            {
                yStart = (int)p.Y - (GlobalState.TerrainPlacementSize / 2);
            }
            else
            {
                yStart = (int)p.Y - (GlobalState.TerrainPlacementSize / 3);
            }

            for (int i = xStart; i < xStart + GlobalState.TerrainPlacementSize; i++)
            {
                for (int j = yStart; j < yStart + GlobalState.TerrainPlacementSize; j++)
                {
                    if (i >= 0 && i <= Editor.RowCount - 1 && j >= 0 && j <= Editor.ColumnCount - 1)
                    {
                        elements.Add(grid.GetCell(i, j));
                    }
                }
            }

            return elements.Distinct();
        }

        private static string SelectedTileType()
        {
            string s = null;
            if (GlobalState.SelectedTile != null)
            {
                s = GlobalState.SelectedTile.Name.Substring("Tile_".Length).ToLower();
            }

            return s;
        }
    }
}
