using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace BoundMaker.Models
{
    public class Explosion : Image, IDisposable
    {
        public static readonly BitmapImage[] Arbiter = new BitmapImage[]
        {
            new BitmapImage(new Uri("/Images/arb/arb0.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/arb/arb1.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/arb/arb2.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/arb/arb3.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/arb/arb4.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/arb/arb5.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/arb/arb6.png", UriKind.RelativeOrAbsolute)),
        };

        public static readonly BitmapImage[] Battlecruiser = new BitmapImage[]
        {
            new BitmapImage(new Uri("/Images/bc/bc0.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/bc/bc1.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/bc/bc2.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/bc/bc3.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/bc/bc4.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/bc/bc5.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/bc/bc6.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/bc/bc7.png", UriKind.RelativeOrAbsolute)),
        };

        public static readonly BitmapImage[] Carrier = new BitmapImage[]
        {
            new BitmapImage(new Uri("/Images/carrier/carrier0.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/carrier/carrier1.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/carrier/carrier2.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/carrier/carrier3.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/carrier/carrier4.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/carrier/carrier5.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/carrier/carrier6.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/carrier/carrier7.png", UriKind.RelativeOrAbsolute)),
        };

        public static readonly BitmapImage[] Overlord = new BitmapImage[]
        {
            new BitmapImage(new Uri("/Images/ovy/ovy0.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/ovy/ovy1.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/ovy/ovy2.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/ovy/ovy3.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/ovy/ovy4.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/ovy/ovy5.png", UriKind.RelativeOrAbsolute)),
        };

        public static readonly BitmapImage[] Scourge = new BitmapImage[]
        {
            new BitmapImage(new Uri("/Images/scourge/scourge0.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/scourge/scourge1.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/scourge/scourge2.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/scourge/scourge3.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/scourge/scourge4.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/scourge/scourge5.png", UriKind.RelativeOrAbsolute)),
        };

        public static readonly BitmapImage[] Wraith = new BitmapImage[]
        {
            new BitmapImage(new Uri("/Images/wraith/wraith0.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/wraith/wraith1.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/wraith/wraith2.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/wraith/wraith3.png", UriKind.RelativeOrAbsolute)),
            new BitmapImage(new Uri("/Images/wraith/wraith4.png", UriKind.RelativeOrAbsolute)),
        };

        private Canvas parent;
        private readonly Timer time;
        private BitmapImage[] explosionType;
        private int frameCount = 0;

        public Explosion(Canvas canvas, MapLocation location, BitmapImage[] type)
        {
            parent = canvas ?? throw new ArgumentNullException(nameof(canvas));
            if (location == null) throw new ArgumentNullException(nameof(location));
            explosionType = type ?? throw new ArgumentNullException(nameof(type));

            IsHitTestVisible = false;
            DetermineSize(location);
            PlaceImage(location.Center);
            time = new Timer();
            try
            {
                time.Interval = 85;
                time.Tick += new EventHandler(OnTimerEvent);
                time.Start();
            }
            catch
            {
                time.Dispose();
                throw;
            }
        }

        private void DetermineSize(MapLocation location)
        {
            if (location.IsSmall)
            {
                Width = 30;
                Height = 30;
            }
            else if (location.IsLarge && explosionType != Overlord)
            {
                Width = 100;
                Height = 100;
            }
            else
            {
                Width = 60;
                Height = 60;
            }
        }

        private void PlaceImage(Point locationCenter)
        {
            double x = locationCenter.X - (Width / 2);
            double y = locationCenter.Y - (Height / 2);
            if (explosionType == Wraith)
            {
                x += 2;
                y -= 1;
            }
            else if (explosionType == Arbiter)
            {
                x += 2;
            }
            else if (explosionType == Overlord)
            {
                x += 1;
                y += 1;
            }
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
        }

        private void OnTimerEvent(object source, EventArgs e)
        {
            if (frameCount < explosionType.Length)
            {
                Source = explosionType[frameCount];
                frameCount++;
            }
            else
            {
                time.Stop();
                parent.Children.Remove(this);
                Dispose();
            }
        }

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
                    time.Dispose();
                }
                disposed = true;
            }
        }
    }
}
