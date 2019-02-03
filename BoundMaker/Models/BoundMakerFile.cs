using BoundMaker.Views;
using System.Collections.Generic;

namespace BoundMaker.Models
{
    public class BoundMakerFile
    {
        public IEnumerable<MapLocation> Locations { get; set; }
        public IEnumerable<MapTerrainTile> Tiles { get; set; }
        public IEnumerable<BoundSequence> Sequences { get; set; }
    }
}
