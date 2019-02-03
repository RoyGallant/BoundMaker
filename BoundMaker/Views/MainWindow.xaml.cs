using BoundMaker.Models;
using BoundMaker.Services;
using BoundMaker.Utilities;
using System;
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
        private const string ProjectStartDate = "Jan 29, 2011";
        private const string OriginalReleaseDate = "April 23, 2011";
        private const string MadeBy = "Roy";
        private readonly string Email = Encoding.UTF8.GetString(new byte[] { 82, 111, 121, 95, 71, 97, 108, 108, 97, 110, 116, 64, 104, 111, 116, 109, 97, 105, 108, 46, 99, 111, 109 });

        private static readonly string[] BetaTesters = new[] { "LeXteR", "Adzz" };

        private bool viewCode = false;
        private string currentFileName = "Untitled.xml";
        private bool fileExists = false;
        private string fullFilePath = "";

        private readonly Version Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        private static string UnsavedIndicator => GlobalState.HasMadeChanges ? "*" : null;

        public MainWindow()
        {
            GlobalState.Locations = new List<MapLocation>();
            GlobalState.Sequences = new List<BoundSequence>();
            InitializeComponent();
            foreach (MapTerrainTile tile in MapEditor.MapTerrain.GetTiles())
            {
                tile.SetWindowInstance(this);
            }
            GlobalState.SelectedTile = Terrain.TileDirt;
            ExplosionPanel.CurrentSequence = new BoundSequence(GlobalState.Locations);
            GlobalState.Sequences.Add(ExplosionPanel.CurrentSequence);
            ExplosionPanel.SetWindowInstance(this);
            TerrainMode.IsChecked = true;
            RefreshTitle();
        }

        public void RefreshTitle()
        {
            Title = $"Bound Maker v{Version.ToString(2)} - {currentFileName}{UnsavedIndicator}";
        }

        private void WindowClosingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
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
            if (GlobalState.IsPlaying)
            {
                ExplosionPanel.PlayPauseSequenceEventHandler(null, null);
            }
            if (e != null && GlobalState.HasMadeChanges && MessageBox.Show($"Do you want save changes to {currentFileName}?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
            ExplosionPanel.CurrentSequence = GlobalState.Sequences[0];
            ExplosionPanel.UpdateNavigation();
            if (e != null)
            {
                currentFileName = "Untitled.xml";
                fileExists = false;
                GlobalState.HasMadeChanges = false;
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
            if (GlobalState.IsPlaying)
            {
                ExplosionPanel.PlayPauseSequenceEventHandler(null, null);
            }

            if (GlobalState.HasMadeChanges && MessageBox.Show($"Do you want save changes to {currentFileName}?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
                    LocationPanel.SetLocationNames();
                    if (GlobalState.Sequences.Count == 0)
                    {
                        GlobalState.Sequences.Add(new BoundSequence());
                    }
                    ExplosionPanel.CurrentSequence = GlobalState.Sequences[0];
                    ExplosionPanel.UpdateNavigation();
                    if (GlobalState.TerrainPanelIsActive)
                    {
                        SetToTerrainMode(null, null);
                    }
                    else if (GlobalState.LocationPanelIsActive)
                    {
                        SetToLocationMode(null, null);
                    }
                    else if (GlobalState.ExplosionPanelIsActive)
                    {
                        SetToExplosionMode(null, null);
                    }
                    currentFileName = dialog.SafeFileName;
                    fullFilePath = dialog.FileName;
                    fileExists = true;
                    GlobalState.HasMadeChanges = false;
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
                if (GlobalState.IsPlaying)
                {
                    ExplosionPanel.PlayPauseSequenceEventHandler(null, null);
                }

                try
                {
                    var fileData = new BoundMakerFile { Locations = GlobalState.Locations, Tiles = MapEditor.MapTerrain.GetTiles(), Sequences = GlobalState.Sequences };
                    XmlHandler.WriteXmlDocument(fullFilePath, fileData);
                    fileExists = true;
                    GlobalState.HasMadeChanges = false;
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
            if (GlobalState.IsPlaying)
            {
                ExplosionPanel.PlayPauseSequenceEventHandler(null, null);
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
                    GlobalState.HasMadeChanges = false;
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
            if (GlobalState.HasMadeChanges && MessageBox.Show($"Do you want save changes to {currentFileName}?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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
            msg.Append($"Bound Maker\nVersion: {Version.ToString(3)}\n\nProject Start Date: {ProjectStartDate}\nMade By: {MadeBy}\nEmail: {Email}\n\n");
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
        private void SetToExplosionMode(object sender, RoutedEventArgs e)
        {
            GlobalState.ExplosionPanelIsActive = true;
            GlobalState.LocationPanelIsActive = false;
            GlobalState.TerrainPanelIsActive = false;
            Terrain.Visibility = Visibility.Hidden;
            LocationPanel.Visibility = Visibility.Hidden;
            ExplosionPanel.Visibility = Visibility.Visible;
            foreach (MapTerrainTile t in MapEditor.MapTerrain.GetTiles())
            {
                t.IsHitTestVisible = false;
            }
            foreach (MapLocation m in GlobalState.Locations)
            {
                m.IsHitTestVisible = true;
                if (ShowLocations.IsChecked == true || !GlobalState.IsPlaying)
                {
                    m.Visibility = Visibility.Visible;
                }
            }
            if (!GlobalState.IsPlaying)
            {
                ExplosionPanel.CurrentSequence.UpdateLocationColors();
            }
        }

        private void SetToTerrainMode(object sender, RoutedEventArgs e)
        {
            GlobalState.ExplosionPanelIsActive = false;
            GlobalState.LocationPanelIsActive = false;
            GlobalState.TerrainPanelIsActive = true;
            Terrain.Visibility = Visibility.Visible;
            LocationPanel.Visibility = Visibility.Hidden;
            ExplosionPanel.Visibility = Visibility.Hidden;
            foreach (MapTerrainTile t in MapEditor.MapTerrain.GetTiles())
            {
                t.IsHitTestVisible = true;
            }
            foreach (MapLocation m in GlobalState.Locations)
            {
                if (!GlobalState.IsPlaying)
                {
                    m.Visibility = Visibility.Hidden;
                }

                m.IsHitTestVisible = false;
            }
        }

        private void SetToLocationMode(object sender, RoutedEventArgs e)
        {
            GlobalState.ExplosionPanelIsActive = false;
            GlobalState.LocationPanelIsActive = true;
            GlobalState.TerrainPanelIsActive = false;
            Terrain.Visibility = Visibility.Hidden;
            LocationPanel.Visibility = Visibility.Visible;
            ExplosionPanel.Visibility = Visibility.Hidden;
            foreach (MapTerrainTile t in MapEditor.MapTerrain.GetTiles())
            {
                t.IsHitTestVisible = true;
            }
            foreach (MapLocation m in GlobalState.Locations)
            {
                m.SetColor(MapLocation.ColorDefault);
                m.IsHitTestVisible = true;
                if (ShowLocations.IsChecked == true || !GlobalState.IsPlaying)
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
            if (GlobalState.IsPlaying)
            {
                if (ShowLocations.IsChecked == false)
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
            if (GlobalState.ExplosionPanelIsActive)
            {
                ExplosionPanel.SequenceNavigationKeyDownEventHandler(sender, e);
            }
        }

        private void CodeToggleEventHandler(object sender, RoutedEventArgs e)
        {
            viewCode = !viewCode;
            if (viewCode)
            {
                if (GlobalState.IsPlaying)
                {
                    ExplosionPanel.PlayPauseSequenceEventHandler(null, null);
                }
                MapWindow.Visibility = Visibility.Hidden;
                CodeWindow.Visibility = Visibility.Visible;
                ToggleCodeGenerationButton.Content = "Switch to Editor Mode";
            }
            else
            {
                MapWindow.Visibility = Visibility.Visible;
                CodeWindow.Visibility = Visibility.Hidden;
                ToggleCodeGenerationButton.Content = "Switch to Code Generation Mode";
                CodeWindow.TriggerOutput.Text = "Trigger output will go here.";
            }
        }
    }
}
