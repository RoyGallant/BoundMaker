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
            if (TerranExplosion.IsChecked == true)
            {
                GlobalState.ExplosionType = "terran";
            }
            else if (ProtossExplosion.IsChecked == true)
            {
                GlobalState.ExplosionType = "protoss";
            }
            else if (ZergExplosion.IsChecked == true)
            {
                GlobalState.ExplosionType = "zerg";
            }
        }

        #region Preview Animation
        private void SetWaitTime(object sender, TextChangedEventArgs e)
        {
            string waitTime = Regex.Replace(WaitInput.Text, "[^0-9]", "");
            WaitInput.Text = waitTime;
            while (waitTime.StartsWith("0"))
            {
                waitTime = waitTime.Substring(1);
            }

            if (string.IsNullOrEmpty(waitTime))
            {
                waitTime = "0";
            }

            WaitCounterDisplay.Content = waitTime + " ms";
            CurrentSequence.WaitTime = int.Parse(waitTime);
            GlobalState.HasMadeChanges = true;
            mainWindow.RefreshTitle();
        }

        public void SetWaitTime(int milliseconds)
        {
            WaitInput.Text = milliseconds.ToString();
            WaitCounterDisplay.Content = milliseconds + " ms";
        }

        public void PlayPauseSequenceEventHandler(object sender, RoutedEventArgs e)
        {
            GlobalState.IsPlaying = !GlobalState.IsPlaying;
            if (GlobalState.IsPlaying)
            {
                ProtossExplosion.IsEnabled = false;
                TerranExplosion.IsEnabled = false;
                ZergExplosion.IsEnabled = false;
                WaitInput.IsEnabled = false;
                UpdateNavigation();
                mainWindow.ShowLocations.IsEnabled = true;
                mainWindow.LocationVisibilityToggleEventHandler(null, null);
                sequenceTimer.Interval = 10;
                sequenceTimer.Start();
                PlayOrPauseSequence.Content = "Stop";
                foreach (MapLocation loc in GlobalState.Locations)
                {
                    loc.SetColor(MapLocation.ColorDefault);
                }
            }
            else
            {
                ProtossExplosion.IsEnabled = true;
                TerranExplosion.IsEnabled = true;
                ZergExplosion.IsEnabled = true;
                WaitInput.IsEnabled = true;
                mainWindow.ShowLocations.IsEnabled = false;
                foreach (MapLocation loc in GlobalState.Locations)
                {
                    loc.Visibility = Visibility.Visible;
                }

                UpdateNavigation();
                sequenceTimer.Stop();
                PlayOrPauseSequence.Content = "Play";
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

                SequenceCounterDisplay.Content = "Sequence #" + (GlobalState.Sequences.IndexOf(CurrentSequence) + 1);
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
            GlobalState.HasMadeChanges = true;
            mainWindow.RefreshTitle();
            UpdateNavigation();
        }

        internal void SequenceNavigationKeyDownEventHandler(object sender, KeyEventArgs e)
        {
            if (WaitInput.IsFocused == false)
            {
                switch (e.Key)
                {
                    case Key.T:
                        TerranExplosion.IsChecked = true;
                        break;
                    case Key.P:
                        ProtossExplosion.IsChecked = true;
                        break;
                    case Key.Z:
                        ZergExplosion.IsChecked = true;
                        break;
                    case Key.OemPlus:
                        if (SequenceNavigationButtonNext.IsEnabled == true)
                        {
                            SequenceNavigationNextButtonEventHandler(null, null);
                        }
                        break;
                    case Key.OemMinus:
                        if (SequenceNavigationButtonPrevious.IsEnabled == true)
                        {
                            SequenceNavigationPreviousButtonEventHandler(null, null);
                        }
                        break;
                    case Key.PageUp:
                        if (SequenceNavigationButtonFirst.IsEnabled == true)
                        {
                            SequenceNavigationFirstButtonEventHandler(null, null);
                        }
                        break;
                    case Key.PageDown:
                        if (SequenceNavigationButtonLast.IsEnabled == true)
                        {
                            SequenceNavigationLastButtonEventHandler(null, null);
                        }
                        break;
                    case Key.Delete:
                        if (SequenceNavigationButtonDelete.IsEnabled == true)
                        {
                            RemoveSequenceEventHandler(null, null);
                        }
                        break;
                    case Key.Insert:
                        if (SequenceNavigationButtonInsert.IsEnabled == true)
                        {
                            SequenceNavigationInsertButtonEventHandler(null, null);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void UpdateNavigation()
        {
            if (GlobalState.IsPlaying || GlobalState.Sequences.Count <= 0)
            {
                SequenceNavigationButtonFirst.IsEnabled = false;
                SequenceNavigationButtonPrevious.IsEnabled = false;
                SequenceNavigationButtonInsert.IsEnabled = false;
                SequenceNavigationButtonNext.IsEnabled = false;
                SequenceNavigationButtonLast.IsEnabled = false;
                SequenceNavigationButtonDelete.IsEnabled = false;
            }
            else
            {
                if (CurrentSequence.IsBlank() && GlobalState.Sequences.Count <= 1)
                {
                    SequenceNavigationButtonDelete.IsEnabled = false;
                }
                else
                {
                    SequenceNavigationButtonDelete.IsEnabled = true;
                }

                if (CurrentSequence == GlobalState.Sequences.First())
                {
                    SequenceNavigationButtonFirst.IsEnabled = false;
                    SequenceNavigationButtonPrevious.IsEnabled = false;
                }
                else
                {
                    SequenceNavigationButtonFirst.IsEnabled = true;
                    SequenceNavigationButtonPrevious.IsEnabled = true;
                }

                if (CurrentSequence == GlobalState.Sequences.Last())
                {
                    SequenceNavigationButtonLast.IsEnabled = false;
                }
                else
                {
                    SequenceNavigationButtonLast.IsEnabled = true;
                }

                if (CurrentSequence.IsBlank() && CurrentSequence == GlobalState.Sequences.Last())
                {
                    SequenceNavigationButtonNext.IsEnabled = false;
                }
                else
                {
                    SequenceNavigationButtonNext.IsEnabled = true;
                }

                if (CurrentSequence.IsBlank())
                {
                    SequenceNavigationButtonInsert.IsEnabled = false;
                }
                else
                {
                    SequenceNavigationButtonInsert.IsEnabled = true;
                }

                SequenceCounterDisplay.Content = "Sequence #" + (GlobalState.Sequences.IndexOf(CurrentSequence) + 1);
                SetWaitTime(CurrentSequence.WaitTime);
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
