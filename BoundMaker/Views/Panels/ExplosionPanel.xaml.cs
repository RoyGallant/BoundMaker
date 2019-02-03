using BoundMaker.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BoundMaker.Views.Panels
{
    public partial class ExplosionPanel : UserControl, IDisposable
    {
        public BoundSequence CurrentSequence { get; set; }
        private System.Windows.Forms.Timer sequenceTimer;
        private MainWindow mainWindow;

        public ExplosionPanel()
        {
            InitializeComponent();
            sequenceTimer = new System.Windows.Forms.Timer();
            try
            {
                sequenceTimer.Tick += new EventHandler(PlayExplosionsEventHandler);
            }
            catch
            {
                sequenceTimer.Dispose();
            }
        }

        public void SetWindowInstance(MainWindow window)
        {
            mainWindow = window;
        }

        public void ExplosionTypeChanged(object sender, RoutedEventArgs e)
        {
            if (Panel_Explosion.Explosion_Unit_Terran.IsChecked == true)
            {
                GlobalState.ExplosionType = "terran";
            }
            else if (Panel_Explosion.Explosion_Unit_Protoss.IsChecked == true)
            {
                GlobalState.ExplosionType = "protoss";
            }
            else if (Panel_Explosion.Explosion_Unit_Zerg.IsChecked == true)
            {
                GlobalState.ExplosionType = "zerg";
            }
        }

        #region Preview Animation
        private void SetWaitTime(object sender, TextChangedEventArgs e)
        {
            string waitTime = Regex.Replace(Wait_Input.Text, "[^0-9]", "");
            Wait_Input.Text = waitTime;
            while (waitTime.StartsWith("0"))
            {
                waitTime = waitTime.Substring(1);
            }

            if (string.IsNullOrEmpty(waitTime))
            {
                waitTime = "0";
            }

            Wait_Counter_Display.Content = waitTime + " ms";
            CurrentSequence.WaitTime = int.Parse(waitTime);
            GlobalState.ChangesMade = true;
            mainWindow.RefreshTitle();
        }

        public void SetWaitTime(int milliseconds)
        {
            Wait_Input.Text = milliseconds.ToString();
            Wait_Counter_Display.Content = milliseconds + " ms";
        }

        public void PlayPauseSequenceEventHandler(object sender, RoutedEventArgs e)
        {
            GlobalState.Playing = !GlobalState.Playing;
            if (GlobalState.Playing)
            {
                Explosion_Unit_Protoss.IsEnabled = false;
                Explosion_Unit_Terran.IsEnabled = false;
                Explosion_Unit_Zerg.IsEnabled = false;
                Wait_Input.IsEnabled = false;
                UpdateNavigation();
                mainWindow.Settings_Show_Locations.IsEnabled = true;
                mainWindow.LocationVisibilityToggleEventHandler(null, null);
                sequenceTimer.Interval = 10;
                sequenceTimer.Start();
                Sequence_PlayPause.Content = "Stop";
                foreach (MapLocation loc in GlobalState.Locations)
                {
                    loc.SetColor(MapLocation.ColorDefault);
                }
            }
            else
            {
                Explosion_Unit_Protoss.IsEnabled = true;
                Explosion_Unit_Terran.IsEnabled = true;
                Explosion_Unit_Zerg.IsEnabled = true;
                Wait_Input.IsEnabled = true;
                mainWindow.Settings_Show_Locations.IsEnabled = false;
                foreach (MapLocation loc in GlobalState.Locations)
                {
                    loc.Visibility = Visibility.Visible;
                }

                UpdateNavigation();
                sequenceTimer.Stop();
                Sequence_PlayPause.Content = "Play";
                CurrentSequence.UpdateLocationColors();
            }
        }

        private void PlayExplosionsEventHandler(object sender, EventArgs e)
        {
            if (GlobalState.Sequences.Count > 1 || (GlobalState.Sequences.Count > 0 && !CurrentSequence.IsBlank()))
            {
                if (CurrentSequence.IsBlank())
                {
                    GlobalState.Sequences.Remove(CurrentSequence);
                    CurrentSequence = GlobalState.Sequences.First();
                    if (CurrentSequence == null)
                    {
                        CurrentSequence = new BoundSequence();
                    }
                }
                CurrentSequence.MakeExplosions(mainWindow.MapCanvas);
                int lapover = 42;
                if (CurrentSequence == GlobalState.Sequences.Last())
                {
                    lapover += 84;
                }

                SetWaitTime(CurrentSequence.WaitTime);
                if (CurrentSequence.WaitTime < 42)
                {
                    lapover += 36;
                }

                Sequence_Counter_Display.Content = "Sequence #" + (GlobalState.Sequences.IndexOf(CurrentSequence) + 1);
                sequenceTimer.Interval = ((int)Math.Ceiling(CurrentSequence.WaitTime / 42.0)) * 42 + lapover;
                if (CurrentSequence != GlobalState.Sequences.Last())
                {
                    CurrentSequence = GlobalState.Sequences[GlobalState.Sequences.IndexOf(CurrentSequence) + 1];
                }
                else
                {
                    CurrentSequence = GlobalState.Sequences.First();
                }
            }
        }
        #endregion

        #region Sequence Navigation
        private void SequenceNavigationFirstButtonEventHandler(object sender, RoutedEventArgs e)
        {
            if (CurrentSequence.IsBlank())
            {
                GlobalState.Sequences.Remove(CurrentSequence);
            }

            CurrentSequence = GlobalState.Sequences.First();
            UpdateNavigation();
        }

        private void SequenceNavigationPreviousButtonEventHandler(object sender, RoutedEventArgs e)
        {
            int seqIndex = GlobalState.Sequences.IndexOf(CurrentSequence);
            if (CurrentSequence.IsBlank())
            {
                GlobalState.Sequences.Remove(CurrentSequence);
            }

            CurrentSequence = GlobalState.Sequences[seqIndex - 1];
            UpdateNavigation();
        }

        private void SequenceNavigationInsertButtonEventHandler(object sender, RoutedEventArgs e)
        {
            int seqIndex = GlobalState.Sequences.IndexOf(CurrentSequence);
            int wait = CurrentSequence.WaitTime;
            if (CurrentSequence.IsBlank())
            {
                GlobalState.Sequences.Remove(CurrentSequence);
            }

            CurrentSequence = new BoundSequence();
            GlobalState.Sequences.Add(CurrentSequence);
            while (GlobalState.Sequences.IndexOf(CurrentSequence) > seqIndex)
            {
                int idx = GlobalState.Sequences.IndexOf(CurrentSequence);
                BoundSequence temp = GlobalState.Sequences[idx - 1];
                GlobalState.Sequences[idx - 1] = CurrentSequence;
                GlobalState.Sequences[idx] = temp;
            }
            CurrentSequence.WaitTime = wait;
            UpdateNavigation();
        }

        private void SequenceNavigationNextButtonEventHandler(object sender, RoutedEventArgs e)
        {
            if (CurrentSequence == GlobalState.Sequences.Last())
            {
                int wait = CurrentSequence.WaitTime;
                CurrentSequence = new BoundSequence(GlobalState.Locations, wait);
                GlobalState.Sequences.Add(CurrentSequence);
            }
            else if (CurrentSequence.IsBlank())
            {
                int seqIndex = GlobalState.Sequences.IndexOf(CurrentSequence);
                GlobalState.Sequences.Remove(CurrentSequence);
                CurrentSequence = GlobalState.Sequences[seqIndex];
            }
            else
            {
                CurrentSequence = GlobalState.Sequences[GlobalState.Sequences.IndexOf(CurrentSequence) + 1];
            }

            UpdateNavigation();
        }

        private void SequenceNavigationLastButtonEventHandler(object sender, RoutedEventArgs e)
        {
            if (CurrentSequence.IsBlank())
            {
                GlobalState.Sequences.Remove(CurrentSequence);
            }

            CurrentSequence = GlobalState.Sequences.Last();
            UpdateNavigation();
        }

        private void RemoveSequenceEventHandler(object sender, RoutedEventArgs e)
        {
            if (GlobalState.Sequences.Count > 1)
            {
                int idx = GlobalState.Sequences.IndexOf(CurrentSequence);
                GlobalState.Sequences.Remove(CurrentSequence);
                if (idx < GlobalState.Sequences.Count)
                {
                    CurrentSequence = GlobalState.Sequences[idx];
                }
                else
                {
                    CurrentSequence = GlobalState.Sequences[idx - 1];
                }
            }
            else
            {
                for (int i = 0; i < CurrentSequence.States.Count; i++)
                {
                    CurrentSequence.States[CurrentSequence.States.ElementAt(i).Key] = "default";
                    CurrentSequence.UpdateLocationColors();
                }
            }
            GlobalState.ChangesMade = true;
            mainWindow.RefreshTitle();
            UpdateNavigation();
        }

        internal void SequenceNavigationKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if (Wait_Input.IsFocused == false)
            {
                if (e.Key == Key.T)
                {
                    Explosion_Unit_Terran.IsChecked = true;
                }
                else if (e.Key == Key.P)
                {
                    Explosion_Unit_Protoss.IsChecked = true;
                }
                else if (e.Key == Key.Z)
                {
                    Explosion_Unit_Zerg.IsChecked = true;
                }
                else if (e.Key == Key.OemPlus && SNav_Next.IsEnabled == true)
                {
                    SequenceNavigationNextButtonEventHandler(null, null);
                }
                else if (e.Key == Key.OemMinus && SNav_Previous.IsEnabled == true)
                {
                    SequenceNavigationPreviousButtonEventHandler(null, null);
                }
                else if (e.Key == Key.PageUp && SNav_First.IsEnabled == true)
                {
                    SequenceNavigationFirstButtonEventHandler(null, null);
                }
                else if (e.Key == Key.PageDown && SNav_Last.IsEnabled == true)
                {
                    SequenceNavigationLastButtonEventHandler(null, null);
                }
                else if (e.Key == Key.Delete && Sequence_Delete.IsEnabled == true)
                {
                    RemoveSequenceEventHandler(null, null);
                }
                else if (e.Key == Key.Insert && SNav_InsertBefore.IsEnabled == true)
                {
                    SequenceNavigationInsertButtonEventHandler(null, null);
                }
            }
        }

        public void UpdateNavigation()
        {
            if (GlobalState.Playing || GlobalState.Sequences.Count <= 0)
            {
                SNav_First.IsEnabled = false;
                SNav_Previous.IsEnabled = false;
                SNav_InsertBefore.IsEnabled = false;
                SNav_Next.IsEnabled = false;
                SNav_Last.IsEnabled = false;
                Sequence_Delete.IsEnabled = false;
            }
            else
            {
                if (CurrentSequence.IsBlank() && GlobalState.Sequences.Count <= 1)
                {
                    Sequence_Delete.IsEnabled = false;
                }
                else
                {
                    Sequence_Delete.IsEnabled = true;
                }

                if (CurrentSequence == GlobalState.Sequences.First())
                {
                    SNav_First.IsEnabled = false;
                    SNav_Previous.IsEnabled = false;
                }
                else
                {
                    SNav_First.IsEnabled = true;
                    SNav_Previous.IsEnabled = true;
                }

                if (CurrentSequence == GlobalState.Sequences.Last())
                {
                    SNav_Last.IsEnabled = false;
                }
                else
                {
                    SNav_Last.IsEnabled = true;
                }

                if (CurrentSequence.IsBlank() && CurrentSequence == GlobalState.Sequences.Last())
                {
                    SNav_Next.IsEnabled = false;
                }
                else
                {
                    SNav_Next.IsEnabled = true;
                }

                if (CurrentSequence.IsBlank())
                {
                    SNav_InsertBefore.IsEnabled = false;
                }
                else
                {
                    SNav_InsertBefore.IsEnabled = true;
                }

                Panel_Explosion.Sequence_Counter_Display.Content = "Sequence #" + (GlobalState.Sequences.IndexOf(CurrentSequence) + 1);
                Panel_Explosion.SetWaitTime(CurrentSequence.WaitTime);
                CurrentSequence.UpdateLocations(GlobalState.Locations);
                CurrentSequence.UpdateLocationColors();
            }
        }
        #endregion

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    sequenceTimer.Dispose();
                }
                disposed = true;
            }
        }
    }
}
