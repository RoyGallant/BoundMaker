using BoundMaker.Models;
using BoundMaker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BoundMaker.Utilities
{
    public static class Extensions
    {
        public static MapTerrainTile GetCell(this Grid grid, double row, double column)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            return (MapTerrainTile)grid.Children[(int)(row * grid.ColumnDefinitions.Count + column)];
        }

        public static MapTerrainTile GetCell(this Grid grid, int row, int column)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            return (MapTerrainTile)grid.Children[row * grid.ColumnDefinitions.Count + column];
        }

        public static void ShowLocation(this IEnumerable<UIElement> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            foreach (UIElement item in elements)
            {
                ((MapTerrainTile)item).LocationPreview();
            }
        }

        public static void HighlightAll(this IEnumerable<MapTerrainTile> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            foreach (MapTerrainTile item in elements)
            {
                item.SetHighlight(true);
            }
        }

        public static void RefreshGrid(this Grid grid)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));

            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
                {
                    grid.GetCell(i, j).SetHighlight(false);
                }
            }
        }

        public static int GetLocationHeight(this IEnumerable<UIElement> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            UIElement e1 = elements.First(x => Grid.GetRow(x) == elements.Max(y => Grid.GetRow(y)));
            UIElement e2 = elements.First(x => Grid.GetRow(x) == elements.Min(y => Grid.GetRow(y)));
            return (Grid.GetRow(e1) - Grid.GetRow(e2) + 1) * Editor.GridSize;
        }

        public static int GetLocationWidth(this IEnumerable<UIElement> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            UIElement e1 = elements.First(x => Grid.GetColumn(x) == elements.Max(y => Grid.GetColumn(y)));
            UIElement e2 = elements.First(x => Grid.GetColumn(x) == elements.Min(y => Grid.GetColumn(y)));
            return (Grid.GetColumn(e1) - Grid.GetColumn(e2) + 1) * Editor.GridSize;
        }

        public static int GetLocationTop(this IEnumerable<UIElement> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            UIElement e1 = elements.First(x => Grid.GetRow(x) == elements.Min(y => Grid.GetRow(y)));
            return Grid.GetRow(e1) * Editor.GridSize;
        }

        public static int GetLocationLeft(this IEnumerable<UIElement> elements)
        {
            if (elements == null) throw new ArgumentNullException(nameof(elements));

            UIElement e1 = elements.First(x => Grid.GetColumn(x) == elements.Min(y => Grid.GetColumn(y)));
            return Grid.GetColumn(e1) * Editor.GridSize;
        }

        public static IEnumerable<MapTerrainTile> GetTiles(this Grid grid)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            
            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
                {
                    yield return grid.GetCell(i, j);
                }
            }
        }

        public static string GetSelectionContent(this Selector selector)
        {
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            return (selector.SelectedValue as ContentControl)?.Content.ToString();
        }
    }
}
