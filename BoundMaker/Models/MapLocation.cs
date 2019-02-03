using BoundMaker.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BoundMaker.Models
{

    public class MapLocation : Grid
    {
        public TextBlock LocationNameBlock { get; private set; }
        public Rectangle Shape { get; private set; }
        internal static SolidColorBrush ColorDefault = new SolidColorBrush(Color.FromArgb(150, 66, 66, 66));
        internal static SolidColorBrush ColorProtoss = new SolidColorBrush(Color.FromArgb(150, 33, 33, 150));
        internal static SolidColorBrush ColorZerg = new SolidColorBrush(Color.FromArgb(150, 150, 33, 33));
        internal static SolidColorBrush ColorTerran = new SolidColorBrush(Color.FromArgb(150, 160, 110, 11));
        internal static SolidColorBrush ColorBorder = new SolidColorBrush(Color.FromArgb(150, 55, 255, 55));
        internal static SolidColorBrush ColorDefaultHighlight = new SolidColorBrush(Color.FromArgb(150, 66, 150, 66));
        internal static SolidColorBrush ColorProtossHighlight = new SolidColorBrush(Color.FromArgb(150, 66, 66, 200));
        internal static SolidColorBrush ColorZergHighlight = new SolidColorBrush(Color.FromArgb(150, 200, 66, 66));
        internal static SolidColorBrush ColorTerranHighlight = new SolidColorBrush(Color.FromArgb(150, 190, 150, 33));

        private MainWindow mainWindow;

        public MapLocation(int left, int top, int width, int height)
        {
            Width = width;
            Height = height;
            Canvas.SetLeft(this, left);
            Canvas.SetTop(this, top);
            Canvas.SetRight(this, Width);
            Canvas.SetBottom(this, Height);
            Shape = new Rectangle
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Fill = ColorDefault,
                Stroke = ColorBorder
            };
            Children.Add(Shape);
            LocationNameBlock = new TextBlock
            {
                Width = Shape.Width - 4,
                Height = Shape.Height - 4
            };
            Canvas.SetLeft(LocationNameBlock, 2);
            LocationNameBlock.Foreground = new SolidColorBrush(Colors.White);
            LocationNameBlock.TextWrapping = TextWrapping.Wrap;
            Children.Add(LocationNameBlock);
            MouseEnter += new MouseEventHandler(MouseEnterEventHandler);
            MouseLeave += new MouseEventHandler(MouseLeaveEventHandler);
            MouseLeftButtonDown += new MouseButtonEventHandler(MouseLeftButtonDownEventHandler);
            MouseRightButtonDown += new MouseButtonEventHandler(MouseRightButtonDownEventHandler);
            MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUpEventHandler);
            MouseRightButtonUp += new MouseButtonEventHandler(MouseRightButtonUpEventHandler);
            MouseMove += new MouseEventHandler(MouseMoveEventHandler);
        }

        public int LocationTileWidth => (int)(Width / Editor.GridSize);
        public int LocationTileHeight => (int)(Height / Editor.GridSize);
        public bool IsSmall => LocationTileWidth == 1 || LocationTileHeight == 1;
        public bool IsLarge => LocationTileWidth >= 3 || LocationTileHeight >= 3;

        public string LocationName { get => LocationNameBlock.Text; set => LocationNameBlock.Text = value; }

        public Point Center => new Point(Canvas.GetLeft(this) + Width / 2, Canvas.GetTop(this) + Height / 2);

        public void SetWindowInstance(MainWindow window)
        {
            mainWindow = window;
        }

        public void SetColor(Brush color)
        {
            Shape.Fill = color;
        }

        public void SetHighlight(bool shouldHighlight)
        {
            if (shouldHighlight)
            {
                if (Shape.Fill == ColorDefault)
                {
                    Shape.Fill = ColorDefaultHighlight;
                }
                else if (Shape.Fill == ColorProtoss)
                {
                    Shape.Fill = ColorProtossHighlight;
                }
                else if (Shape.Fill == ColorZerg)
                {
                    Shape.Fill = ColorZergHighlight;
                }
                else if (Shape.Fill == ColorTerran)
                {
                    Shape.Fill = ColorTerranHighlight;
                }
            }
            else
            {
                if (Shape.Fill == ColorDefaultHighlight)
                {
                    Shape.Fill = ColorDefault;
                }
                else if (Shape.Fill == ColorProtossHighlight)
                {
                    Shape.Fill = ColorProtoss;
                }
                else if (Shape.Fill == ColorZergHighlight)
                {
                    Shape.Fill = ColorZerg;
                }
                else if (Shape.Fill == ColorTerranHighlight)
                {
                    Shape.Fill = ColorTerran;
                }
            }
        }

        private void MouseEnterEventHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is MapLocation location)) return;

            if (GlobalState.ExplosionPanelIsActive && !GlobalState.IsPlaying)
            {
                location.SetHighlight(true);
            }
            else if (!GlobalState.IsDraggingLocation && !GlobalState.IsResizingLocation)
            {
                location.SetHighlight(true);
            }
        }

        private void MouseLeaveEventHandler(object sender, MouseEventArgs e)
        {
            if (!(sender is MapLocation location)) return;

            if (GlobalState.ExplosionPanelIsActive)
            {
                location.SetHighlight(false);
            }
            else
            {
                if (!GlobalState.IsResizingLocation)
                {
                    Cursor = Cursors.Arrow;
                }

                if (!GlobalState.IsDraggingLocation)
                {
                    location.SetHighlight(false);
                }
            }
        }

        private void MouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is MapLocation location)) return;

            if (GlobalState.ExplosionPanelIsActive && !GlobalState.IsPlaying)
            {
                mainWindow.ExplosionPanel.CurrentSequence.SetLocationState(location, GlobalState.ExplosionType);
                mainWindow.ExplosionPanel.UpdateNavigation();
            }
            else
            {
                if (location.EdgeSelected(e) && GlobalState.LocationPanelIsActive)
                {
                    GlobalState.IsResizingLocation = true;
                    GlobalState.IsDraggingLocation = false;
                    mainWindow.MapEditor.DragLocation = location;
                    mainWindow.MapEditor.DragStart = e.GetPosition(mainWindow.MapCanvas);
                }
                else
                {
                    mainWindow.MapEditor.DragLocation = location;
                    GlobalState.IsDraggingLocation = true;
                    GlobalState.IsResizingLocation = false;
                    mainWindow.MapEditor.DragStart = mainWindow.MapEditor.DragLocation.Center;
                }
            }
            GlobalState.HasMadeChanges = true;
            mainWindow.RefreshTitle();
        }

        private void MouseLeftButtonUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            MouseHoverEventHandler(sender as MapLocation, e);
            foreach (BoundSequence seq in GlobalState.Sequences)
            {
                seq.UpdateLocations(GlobalState.Locations);
            }
            GlobalState.IsDraggingLocation = false;
            GlobalState.IsResizingLocation = false;
        }

        private void MouseRightButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
        }

        private void MouseRightButtonUpEventHandler(object sender, MouseButtonEventArgs e)
        {
            if (GlobalState.ExplosionPanelIsActive && !GlobalState.IsPlaying)
            {
                mainWindow.ExplosionPanel.CurrentSequence.SetLocationState(this, "default");
                mainWindow.ExplosionPanel.UpdateNavigation();
            }
            else if (!GlobalState.IsDraggingLocation && !GlobalState.IsPlaying)
            {
                GlobalState.Locations.Remove(this);
                mainWindow.MapCanvas.Children.Remove(this);
                mainWindow.LocationPanel.SetLocationNames();
                foreach (BoundSequence item in GlobalState.Sequences)
                {
                    item.UpdateLocations(GlobalState.Locations);
                }
            }
            GlobalState.HasMadeChanges = true;
            mainWindow.RefreshTitle();
        }

        public void MouseMoveEventHandler(object sender, MouseEventArgs e)
        {
            if (GlobalState.IsDraggingLocation)
            {
                mainWindow.MapEditor.DragEventHandler(e);
            }
            else if (GlobalState.IsResizingLocation)
            {
                mainWindow.MapEditor.ResizeEventHandler(e);
            }
            else
            {
                if (sender is MapLocation location)
                {
                    MouseHoverEventHandler(location, e);
                }
            }
        }

        public void MouseHoverEventHandler(MapLocation location, MouseEventArgs e)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            foreach (MapLocation loc in GlobalState.Locations)
            {
                loc.SetHighlight(false);
            }
            location.SetHighlight(true);
            if (GlobalState.LocationPanelIsActive)
            {
                GlobalState.IsResizingLeft = location.EdgeSelectedLeft(e);
                GlobalState.IsResizingRight = location.EdgeSelectedRight(e);
                GlobalState.IsResizingUp = location.EdgeSelectedUp(e);
                GlobalState.IsResizingDown = location.EdgeSelectedDown(e);
                if (GlobalState.IsResizingLeft || GlobalState.IsResizingRight || GlobalState.IsResizingUp || GlobalState.IsResizingDown)
                {
                    if ((GlobalState.IsResizingLeft && GlobalState.IsResizingUp) || (GlobalState.IsResizingRight && GlobalState.IsResizingDown))
                    {
                        // Top Left or Bottom Right
                        Cursor = Cursors.SizeNWSE;
                    }
                    else if ((GlobalState.IsResizingRight && GlobalState.IsResizingUp) || (GlobalState.IsResizingLeft && GlobalState.IsResizingDown))
                    {
                        // Top Right or Bottom Left
                        Cursor = Cursors.SizeNESW;
                    }
                    else if (GlobalState.IsResizingLeft || GlobalState.IsResizingRight)
                    {
                        // Left or Right
                        Cursor = Cursors.SizeWE;
                    }
                    else if (GlobalState.IsResizingUp || GlobalState.IsResizingDown)
                    {
                        // Top or Bottom
                        Cursor = Cursors.SizeNS;
                    }
                }
                else
                {
                    // Middle
                    Cursor = Cursors.SizeAll;
                }
            }
        }

        private bool EdgeSelected(MouseEventArgs e)
        {
            return EdgeSelectedLeft(e) || EdgeSelectedRight(e) || EdgeSelectedUp(e) || EdgeSelectedDown(e);
        }

        private bool EdgeSelectedLeft(MouseEventArgs e)
        {
            return e.GetPosition(this).X < Editor.GridSize / 4.0;
        }

        private bool EdgeSelectedRight(MouseEventArgs e)
        {
            return e.GetPosition(this).X > Width - Editor.GridSize / 4.0;
        }

        private bool EdgeSelectedUp(MouseEventArgs e)
        {
            return e.GetPosition(this).Y < Editor.GridSize / 4.0;
        }

        private bool EdgeSelectedDown(MouseEventArgs e)
        {
            return e.GetPosition(this).Y > Height - Editor.GridSize / 4.0;
        }
    }
}
