using BoundMaker.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BoundMaker
{
    public static class GlobalState
    {
        internal static Label SelectedTile { get; set; }
        internal static bool Playing { get; set; }
        internal static List<MapLocation> Locations { get; set; }
        internal static List<BoundSequence> Sequences { get; set; }
        internal static bool DraggingLocation { get; set; }
        internal static bool ResizingLocation { get; set; }
        internal static bool Mode_Explosion { get; set; }
        internal static bool Mode_Location { get; set; }
        internal static bool Mode_Terrain { get; set; }
        internal static int TerrainPlacementSize { get; set; }
        internal static string ExplosionType { get; set; }
        internal static Point LocationStart { get; set; }
        internal static bool ResizeLeft { get; set; }
        internal static bool ResizeRight { get; set; }
        internal static bool ResizeUp { get; set; }
        internal static bool ResizeDown { get; set; }
        internal static bool ChangesMade { get; set; }
    }
}
