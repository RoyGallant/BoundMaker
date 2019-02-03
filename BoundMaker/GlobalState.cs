using BoundMaker.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace BoundMaker
{
    public static class GlobalState
    {
        internal static Label SelectedTile { get; set; }
        internal static bool IsPlaying { get; set; }
        internal static List<MapLocation> Locations { get; set; }
        internal static List<BoundSequence> Sequences { get; set; }
        internal static bool IsDraggingLocation { get; set; }
        internal static bool IsResizingLocation { get; set; }
        internal static bool ExplosionPanelIsActive { get; set; }
        internal static bool LocationPanelIsActive { get; set; }
        internal static bool TerrainPanelIsActive { get; set; }
        internal static int TerrainPlacementSize { get; set; }
        internal static string ExplosionType { get; set; }
        internal static Point LocationStart { get; set; }
        internal static bool IsResizingLeft { get; set; }
        internal static bool IsResizingRight { get; set; }
        internal static bool IsResizingUp { get; set; }
        internal static bool IsResizingDown { get; set; }
        internal static bool HasMadeChanges { get; set; }
    }
}
