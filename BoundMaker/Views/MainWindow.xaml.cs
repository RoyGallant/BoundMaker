using BoundMaker.Models;
using BoundMaker.Services;
using BoundMaker.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BoundMaker.Views
{
    public partial class MainWindow : Window
    {
        public const string ProjectStartDate = "Jan 29, 2011";
        public const string OriginalReleaseDate = "April 23, 2011";
        public const string MadeBy = "Roy";
        public readonly string Email = Encoding.UTF8.GetString(new byte[] { 82, 111, 121, 95, 71, 97, 108, 108, 97, 110, 116, 64, 104, 111, 116, 109, 97, 105, 108, 46, 99, 111, 109 });

        public static readonly string[] BetaTesters = new[] { "LeXteR", "Adzz" };

        private bool viewCode = false;
        private string currentFileName = "Untitled.xml";
        private bool fileExists = false;
        private string fullFilePath = "";

        private readonly string Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        private static string UnsavedIndicator => GlobalState.ChangesMade ? "*" : null;

        public MainWindow()
        {
            GlobalState.Locations = new List<MapLocation>();
            GlobalState.Sequences = new List<BoundSequence>();
            InitializeComponent();
            foreach (MapTerrainTile tile in MapEditor.MapTerrain.GetTiles())
            {
                tile.SetWindowInstance(this);
            }
            GlobalState.SelectedTile = Panel_Terrain.Tile_Dirt;
            Panel_Explosion.CurrentSequence = new BoundSequence(GlobalState.Locations);
            GlobalState.Sequences.Add(Panel_Explosion.CurrentSequence);
            Panel_Explosion.SetWindowInstance(this);
            Mode_Terrain.IsChecked = true;
            RefreshTitle();
        }

        public void RefreshTitle()
        {
            Title = $"Bound Maker v{Version} - {currentFileName}{UnsavedIndicator}";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseExecuted(null, null);
        }

        #region Menu
        private void NewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalState.Playing)
            {
                Panel_Explosion.PlayPauseSequenceEventHandler(null, null);
            }
            if (e != null && GlobalState.ChangesMade && MessageBox.Show($"Do you want save changes to {currentFileName}?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (fileExists)
                {
                    SaveExecuted(sender, e);
                }
                else
                {
                    SaveAsExecuted(sender, e);
                }
            }
            foreach (MapTerrainTile tile in MapEditor.MapTerrain.GetTiles())
            {
                tile.SetTerrain("null");
            }
            foreach (MapLocation loc in GlobalState.Locations)
            {
                MapEditor.MapCanvas.Children.Remove(loc);
            }
            GlobalState.Locations = new List<MapLocation>();
            GlobalState.Sequences = new List<BoundSequence>
            {
                new BoundSequence()
            };
            Panel_Explosion.CurrentSequence = GlobalState.Sequences[0];
            Panel_Explosion.UpdateNavigation();
            if (e != null)
            {
                currentFileName = "Untitled.xml";
                fileExists = false;
                GlobalState.ChangesMade = false;
                RefreshTitle();
            }
        }

        private void OpenCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalState.Playing)
            {
                Panel_Explosion.PlayPauseSequenceEventHandler(null, null);
            }

            if (GlobalState.ChangesMade && MessageBox.Show($"Do you want save changes to {currentFileName}?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (fileExists)
                {
                    SaveExecuted(sender, e);
                }
                else
                {
                    SaveAsExecuted(sender, e);
                }
            }
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".xml",
                Filter = "Extensible Markup Language File|*.xml"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                List<MapLocation> backupLocations = GlobalState.Locations;
                List<BoundSequence> backupSequences = GlobalState.Sequences;
                try
                {
                    BoundMakerFile fileData = XmlHandler.ReadXmlDocument(filename);
                    NewExecuted(null, null);
                    GlobalState.Locations = fileData.Locations.ToList();
                    GlobalState.Sequences = fileData.Sequences.ToList();
                    foreach (MapTerrainTile newTile in fileData.Tiles)
                    {
                        MapEditor.MapTerrain.GetCell(Grid.GetRow(newTile), Grid.GetColumn(newTile)).SetTerrain(newTile.Terrain);
                    }
                    foreach (MapLocation m in GlobalState.Locations)
                    {
                        MapEditor.MapCanvas.Children.Add(m);
                        m.SetWindowInstance(this);
                    }
                    Panel_Location.SetLocationNames();
                    if (GlobalState.Sequences.Count == 0)
                    {
                        GlobalState.Sequences.Add(new BoundSequence());
                    }
                    Panel_Explosion.CurrentSequence = GlobalState.Sequences[0];
                    Panel_Explosion.UpdateNavigation();
                    if (GlobalState.Mode_Terrain)
                    {
                        Mode_Terrain_Checked(null, null);
                    }
                    else if (GlobalState.Mode_Location)
                    {
                        Mode_Location_Checked(null, null);
                    }
                    else if (GlobalState.Mode_Explosion)
                    {
                        Mode_Explosion_Checked(null, null);
                    }
                    currentFileName = dialog.SafeFileName;
                    fullFilePath = dialog.FileName;
                    fileExists = true;
                    GlobalState.ChangesMade = false;
                    RefreshTitle();
                }
                catch
                {
                    GlobalState.Locations = backupLocations;
                    GlobalState.Sequences = backupSequences;
                    MessageBox.Show("Failed to read file. And I tried really hard, too :(");
                }
            }
            e.Handled = true;
        }

        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (fileExists)
            {
                if (GlobalState.Playing)
                {
                    Panel_Explosion.PlayPauseSequenceEventHandler(null, null);
                }

                try
                {
                    var fileData = new BoundMakerFile { Locations = GlobalState.Locations, Tiles = MapEditor.MapTerrain.GetTiles(), Sequences = GlobalState.Sequences };
                    XmlHandler.WriteXmlDocument(fullFilePath, fileData);
                    fileExists = true;
                    GlobalState.ChangesMade = false;
                    RefreshTitle();
                }
                catch
                {
                    MessageBox.Show("Failed to save file. Try 'Save As'.");
                }
            }
            else
            {
                SaveAsExecuted(sender, e);
            }
        }

        private void SaveAsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void SaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalState.Playing)
            {
                Panel_Explosion.PlayPauseSequenceEventHandler(null, null);
            }

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = currentFileName,
                DefaultExt = ".xml",
                Filter = "Extensible Markup Language File|*.xml"
            };
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;
                try
                {
                    var fileData = new BoundMakerFile { Locations = GlobalState.Locations, Tiles = MapEditor.MapTerrain.GetTiles(), Sequences = GlobalState.Sequences };
                    XmlHandler.WriteXmlDocument(filename, fileData);
                    currentFileName = dlg.SafeFileName;
                    fullFilePath = dlg.FileName;
                    fileExists = true;
                    GlobalState.ChangesMade = false;
                    RefreshTitle();
                }
                catch
                {
                    MessageBox.Show("Failed to save file. Sorry :(");
                }
            }
        }

        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (GlobalState.ChangesMade && MessageBox.Show($"Do you want save changes to {currentFileName}?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (fileExists)
                {
                    SaveExecuted(sender, e);
                }
                else
                {
                    SaveAsExecuted(sender, e);
                }
            }
            Application.Current.Shutdown();
        }

        private void HelpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var msg = new StringBuilder();
            msg.AppendLine("General How-To:\n");
            msg.AppendLine("Left-click to place, right-click to remove.\n\n");
            msg.AppendLine("Terrain:");
            msg.AppendLine("Left-click to place selected terrain.\nRight-click to set terrain to null.\n");
            msg.AppendLine("Location:");
            msg.AppendLine("Left-click on a tile and drag to draw a location.\nLeft-click on a location and drag to move the location.\nLeft-click on a location edge and drag to resize the location.\nRight-click to remove a location.\n");
            msg.AppendLine("Explosion:");
            msg.AppendLine("Left-click on a location to place an explosion.\nRight-click to remove an explosion.");
            MessageBox.Show(msg.ToString(), "Help");
            e.Handled = true;
        }

        private void AboutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void AboutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var msg = new StringBuilder();
            msg.Append($"Bound Maker\nVersion: {Version}\n\nProject Start Date: {ProjectStartDate}\nMade By: {MadeBy}\nEmail: {Email}\n\n");
            msg.AppendLine("Beta Testers:");
            foreach (string tester in BetaTesters)
            {
                msg.Append($" - {tester}\n");
            }
            msg.AppendLine("\n\nAll explosion and tile images are property of Blizzard Entertainment.\nThis program is free for personal use.");
            MessageBox.Show(msg.ToString(), "About");
            e.Handled = true;
        }
        public Canvas MapCanvas => MapEditor.MapCanvas;

        public Grid MapTerrain => MapEditor.MapTerrain;
        #endregion

        #region Mode Toggles
        private void Mode_Explosion_Checked(object sender, RoutedEventArgs e)
        {
            GlobalState.Mode_Explosion = true;
            GlobalState.Mode_Location = false;
            GlobalState.Mode_Terrain = false;
            Panel_Terrain.Visibility = Visibility.Hidden;
            Panel_Location.Visibility = Visibility.Hidden;
            Panel_Explosion.Visibility = Visibility.Visible;
            foreach (MapTerrainTile t in MapEditor.MapTerrain.GetTiles())
            {
                t.IsHitTestVisible = false;
            }
            foreach (MapLocation m in GlobalState.Locations)
            {
                m.IsHitTestVisible = true;
                if (Settings_Show_Locations.IsChecked == true || !GlobalState.Playing)
                {
                    m.Visibility = Visibility.Visible;
                }
            }
            if (!GlobalState.Playing)
            {
                Panel_Explosion.CurrentSequence.UpdateLocationColors();
            }
        }

        private void Mode_Terrain_Checked(object sender, RoutedEventArgs e)
        {
            GlobalState.Mode_Explosion = false;
            GlobalState.Mode_Location = false;
            GlobalState.Mode_Terrain = true;
            Panel_Terrain.Visibility = Visibility.Visible;
            Panel_Location.Visibility = Visibility.Hidden;
            Panel_Explosion.Visibility = Visibility.Hidden;
            foreach (MapTerrainTile t in MapEditor.MapTerrain.GetTiles())
            {
                t.IsHitTestVisible = true;
            }
            foreach (MapLocation m in GlobalState.Locations)
            {
                if (!GlobalState.Playing)
                {
                    m.Visibility = Visibility.Hidden;
                }

                m.IsHitTestVisible = false;
            }
        }

        private void Mode_Location_Checked(object sender, RoutedEventArgs e)
        {
            GlobalState.Mode_Explosion = false;
            GlobalState.Mode_Location = true;
            GlobalState.Mode_Terrain = false;
            Panel_Terrain.Visibility = Visibility.Hidden;
            Panel_Location.Visibility = Visibility.Visible;
            Panel_Explosion.Visibility = Visibility.Hidden;
            foreach (MapTerrainTile t in MapEditor.MapTerrain.GetTiles())
            {
                t.IsHitTestVisible = true;
            }
            foreach (MapLocation m in GlobalState.Locations)
            {
                m.SetColor(MapLocation.ColorDefault);
                m.IsHitTestVisible = true;
                if (Settings_Show_Locations.IsChecked == true || !GlobalState.Playing)
                {
                    m.Visibility = Visibility.Visible;
                }
            }
        }
        #endregion

        #region Setting Toggles
        private void GridVisibilityToggleEventHandler(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).IsChecked == false)
            {
                foreach (MapTerrainTile t in MapEditor.MapTerrain.GetTiles())
                {
                    t.TileBorder.BorderThickness = new Thickness(0);
                }
            }
            else
            {
                foreach (MapTerrainTile t in MapEditor.MapTerrain.GetTiles())
                {
                    t.TileBorder.BorderThickness = new Thickness(.1);
                }
            }
        }

        public void LocationVisibilityToggleEventHandler(object sender, RoutedEventArgs e)
        {
            if (GlobalState.Playing)
            {
                if (Settings_Show_Locations.IsChecked == false)
                {
                    foreach (MapLocation loc in GlobalState.Locations)
                    {
                        loc.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    foreach (MapLocation loc in GlobalState.Locations)
                    {
                        loc.Visibility = Visibility.Visible;
                    }
                }
            }
        }
        #endregion

        private void WindowKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if (GlobalState.Mode_Explosion)
            {
                Panel_Explosion.SequenceNavigationKeyDownEventHandler(sender, e);
            }
        }

        private void CodeToggleEventHandler(object sender, RoutedEventArgs e)
        {
            viewCode = !viewCode;
            if (viewCode)
            {
                if (GlobalState.Playing)
                {
                    Panel_Explosion.PlayPauseSequenceEventHandler(null, null);
                }
                MapWindow.Visibility = Visibility.Hidden;
                CodeWindow.Visibility = Visibility.Visible;
                Toggle_Code.Content = "Switch to Editor Mode";
            }
            else
            {
                MapWindow.Visibility = Visibility.Visible;
                CodeWindow.Visibility = Visibility.Hidden;
                Toggle_Code.Content = "Switch to Code Generation Mode";
                CodeWindow.Code_Output.Text = "Trigger output will go here.";
            }
        }
    }
}
