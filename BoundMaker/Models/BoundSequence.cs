using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace BoundMaker.Models
{
    public class BoundSequence
    {
        public Dictionary<MapLocation, string> States { get; private set; }
        public int WaitTime { get; set; }

        public BoundSequence()
        {
            States = new Dictionary<MapLocation, string>();
            WaitTime = 0;
        }

        public BoundSequence(IEnumerable<MapLocation> locations)
        {
            States = new Dictionary<MapLocation, string>();
            UpdateLocations(locations);
            WaitTime = 0;
        }

        public BoundSequence(IEnumerable<MapLocation> locations, int wait)
        {
            States = new Dictionary<MapLocation, string>();
            UpdateLocations(locations);
            WaitTime = wait;
        }

        public void UpdateLocations(IEnumerable<MapLocation> locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException(nameof(locations));
            }

            foreach (MapLocation m in locations)
            {
                if (!States.Keys.Contains(m))
                {
                    States[m] = "default";
                }
            }
            var old = States.Keys.Where(x => locations.Contains(x) == false).ToList();
            foreach (MapLocation m in old)
            {
                States.Remove(m);
            }
        }

        public void SetLocationState(MapLocation location, string state)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state != "default" && location.IsSmall)
            {
                state = "zerg";
            }

            States[location] = state;
            UpdateLocationColors();
        }

        public void UpdateLocationColors()
        {
            foreach (MapLocation m in States.Keys)
            {
                if (States[m] == "zerg")
                {
                    m.SetColor(MapLocation.ColorZerg);
                }
                else if (States[m] == "terran")
                {
                    m.SetColor(MapLocation.ColorTerran);
                }
                else if (States[m] == "protoss")
                {
                    m.SetColor(MapLocation.ColorProtoss);
                }
                else
                {
                    m.SetColor(MapLocation.ColorDefault);
                }
            }
        }

        public bool IsBlank()
        {
            foreach (MapLocation m in States.Keys)
            {
                if (States[m] != "default")
                {
                    return false;
                }
            }
            return true;
        }

        public string GetExplosionUnitName(MapLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            string s = "";
            if (location.IsSmall)
            {
                s = "Zerg Scourge";
            }
            else if (States[location] == "zerg")
            {
                s = "Zerg Overlord";
            }
            else if (States[location] == "terran")
            {
                if (location.IsLarge)
                {
                    s = "Terran Battlecruiser";
                }
                else
                {
                    s = "Terran Wraith";
                }
            }
            else if (States[location] == "protoss")
            {
                if (location.IsLarge)
                {
                    s = "Protoss Carrier";
                }
                else
                {
                    s = "Protoss Arbiter";
                }
            }
            return s;
        }

        public void MakeExplosions(Canvas canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            foreach (MapLocation loc in States.Keys.Where(x => States[x] != "default"))
            {
                if (loc.IsSmall)
                {
                    canvas.Children.Add(new Explosion(canvas, loc, Explosion.Scourge));
                }
                else if (States[loc] == "zerg")
                {
                    canvas.Children.Add(new Explosion(canvas, loc, Explosion.Overlord));
                }
                else if (States[loc] == "terran")
                {
                    if (loc.IsLarge)
                    {
                        canvas.Children.Add(new Explosion(canvas, loc, Explosion.Battlecruiser));
                    }
                    else
                    {
                        canvas.Children.Add(new Explosion(canvas, loc, Explosion.Wraith));
                    }
                }
                else if (States[loc] == "protoss")
                {
                    if (loc.IsLarge)
                    {
                        canvas.Children.Add(new Explosion(canvas, loc, Explosion.Carrier));
                    }
                    else
                    {
                        canvas.Children.Add(new Explosion(canvas, loc, Explosion.Arbiter));
                    }
                }
            }
        }
    }
}
