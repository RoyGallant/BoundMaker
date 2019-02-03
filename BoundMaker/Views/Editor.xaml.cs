using BoundMaker.Models;
using BoundMaker.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BoundMaker.Views
{
    public partial class Editor : UserControl
    {
        public const int RowCount = 16;
        public const int ColumnCount = 29;
        public const int GridSize = 25;
        public const int AspectRatio = ColumnCount / RowCount;

        public MapLocation DragLocation { get; set; }
        public Point DragStart;

        public Editor()
        {
            InitializeComponent();
            MakeMap();
            MapCanvas.MouseMove += new MouseEventHandler(MouseMoveEventHandler);
            MapCanvas.MouseLeave += new MouseEventHandler(MouseLeaveEventHandler);
            MapCanvas.MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUpEventHandler);
        }


        private void MakeMap()
        {
            for (int row = 0; row < RowCount; row++)
            {
                MapTerrain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(GridSize, GridUnitType.Pixel) });
            }
            for (int col = 0; col < ColumnCount; col++)
            {
                MapTerrain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(GridSize, GridUnitType.Pixel) });
            }
            for (int row = 0; row < MapTerrain.RowDefinitions.Count; row++)
            {
                for (int col = 0; col < MapTerrain.ColumnDefinitions.Count; col++)
                {
                    var tile = new MapTerrainTile();
                    MapTerrain.Children.Add(tile);
                    Grid.SetRow(tile, row);
                    Grid.SetColumn(tile, col);
                }
            }
        }

        private void MouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            MapTerrain.RefreshGrid();
            Cursor = Cursors.Arrow;
        }

        private void MouseLeftButtonUpEventHandler(object sender, MouseEventArgs e)
        {
            if (GlobalState.Playing)
            {
                foreach (MapLocation loc in GlobalState.Locations)
                {
                    loc.SetHighlight(false);
                }
                Cursor = Cursors.Arrow;
            }
            GlobalState.DraggingLocation = false;
            GlobalState.ResizingLocation = false;
        }

        private void MouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            if (GlobalState.ResizingLocation)
            {
                ResizeEventHandler(e);
            }
            else if (GlobalState.DraggingLocation)
            {
                DragEventHandler(e);
            }
        }

        public void DragEventHandler(MouseEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            Point temp = e.GetPosition(MapCanvas);
            while (temp.X - DragStart.X >= GridSize / 2 + 1 && Canvas.GetLeft(DragLocation) + DragLocation.Width <= MapCanvas.Width - GridSize)
            {
                Canvas.SetLeft(DragLocation, Canvas.GetLeft(DragLocation) + GridSize);
                DragStart.X += GridSize;
            }
            while (temp.X - DragStart.X <= -GridSize / 2 + 1 && Canvas.GetLeft(DragLocation) >= GridSize)
            {
                Canvas.SetLeft(DragLocation, Canvas.GetLeft(DragLocation) - GridSize);
                DragStart.X -= GridSize;
            }
            while (temp.Y - DragStart.Y >= GridSize / 2 + 1 && Canvas.GetTop(DragLocation) + DragLocation.Height <= MapCanvas.Height - GridSize)
            {
                Canvas.SetTop(DragLocation, Canvas.GetTop(DragLocation) + GridSize);
                DragStart.Y += GridSize;
            }
            while (temp.Y - DragStart.Y <= -GridSize / 2 + 1 && Canvas.GetTop(DragLocation) >= GridSize)
            {
                Canvas.SetTop(DragLocation, Canvas.GetTop(DragLocation) - GridSize);
                DragStart.Y -= GridSize;
            }
            DragLocation.SetHighlight(true);
        }

        public void ResizeEventHandler(MouseEventArgs e)
        {
            if (e == null) throw new ArgumentNullException(nameof(e));

            DragLocation.SetHighlight(true);
            Point temp = e.GetPosition(MapCanvas);
            if (GlobalState.ResizeLeft)
            {
                while (temp.X - DragStart.X >= GridSize / 2 + 1 && DragLocation.Width > GridSize)
                {
                    Canvas.SetLeft(DragLocation, Canvas.GetLeft(DragLocation) + GridSize);
                    DragLocation.Width -= GridSize;
                    DragStart.X += GridSize;
                }
                while (temp.X - DragStart.X <= -GridSize / 2 + 1)
                {
                    Canvas.SetLeft(DragLocation, Canvas.GetLeft(DragLocation) - GridSize);
                    DragLocation.Width += GridSize;
                    DragStart.X -= GridSize;
                }
                if (temp.X - DragStart.X >= GridSize * 1.5 + 1 && DragLocation.Width <= GridSize)
                {
                    Canvas.SetLeft(DragLocation, Canvas.GetLeft(DragLocation) + GridSize);
                    GlobalState.ResizeLeft = false;
                    GlobalState.ResizeRight = true;
                    DragStart.X += GridSize * 2;
                }
            }
            else if (GlobalState.ResizeRight)
            {
                while (temp.X - DragStart.X >= GridSize / 2 + 1)
                {
                    DragLocation.Width += GridSize;
                    DragStart.X += GridSize;
                }
                while (temp.X - DragStart.X <= -GridSize / 2 + 1 && DragLocation.Width > GridSize)
                {
                    DragLocation.Width -= GridSize;
                    DragStart.X -= GridSize;
                }
                if (temp.X - DragStart.X <= -GridSize * 1.5 + 1 && DragLocation.Width <= GridSize)
                {
                    Canvas.SetLeft(DragLocation, Canvas.GetLeft(DragLocation) - GridSize);
                    GlobalState.ResizeLeft = true;
                    GlobalState.ResizeRight = false;
                    DragStart.X -= GridSize * 2;
                }
            }
            if (GlobalState.ResizeUp)
            {
                while (temp.Y - DragStart.Y >= GridSize / 2 + 1 && DragLocation.Height > GridSize)
                {
                    Canvas.SetTop(DragLocation, Canvas.GetTop(DragLocation) + GridSize);
                    DragLocation.Height -= GridSize;
                    DragStart.Y += GridSize;
                }
                while (temp.Y - DragStart.Y <= -GridSize / 2 + 1)
                {
                    Canvas.SetTop(DragLocation, Canvas.GetTop(DragLocation) - GridSize);
                    DragLocation.Height += GridSize;
                    DragStart.Y -= GridSize;
                }
                if (temp.Y - DragStart.Y >= GridSize * 1.5 + 1 && DragLocation.Height <= GridSize)
                {
                    Canvas.SetTop(DragLocation, Canvas.GetTop(DragLocation) + GridSize);
                    GlobalState.ResizeUp = false;
                    GlobalState.ResizeDown = true;
                    DragStart.Y += GridSize * 2;
                }
            }
            else if (GlobalState.ResizeDown)
            {
                while (temp.Y - DragStart.Y >= GridSize / 2 + 1)
                {
                    DragLocation.Height += GridSize;
                    DragStart.Y += GridSize;
                }
                while (temp.Y - DragStart.Y <= -GridSize / 2 + 1 && DragLocation.Height > GridSize)
                {
                    DragLocation.Height -= GridSize;
                    DragStart.Y -= GridSize;
                }
                if (temp.Y - DragStart.Y <= -GridSize * 1.5 + 1 && DragLocation.Height <= GridSize)
                {
                    Canvas.SetTop(DragLocation, Canvas.GetTop(DragLocation) - GridSize);
                    GlobalState.ResizeUp = true;
                    GlobalState.ResizeDown = false;
                    DragStart.Y -= GridSize * 2;
                }
            }
        }

    }
}
