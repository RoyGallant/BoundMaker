using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace BoundMaker.Views.Panels
{
    public partial class TerrainPanel : UserControl
    {
        public TerrainPanel()
        {
            InitializeComponent();
        }

        private void TerrainSizeChanged(object sender, RoutedEventArgs e)
        {
            if (Tile_Size_1x1.IsChecked == true)
            {
                GlobalState.TerrainPlacementSize = 1;
            }
            else if (Tile_Size_2x2.IsChecked == true)
            {
                GlobalState.TerrainPlacementSize = 2;
            }
            else if (Tile_Size_3x3.IsChecked == true)
            {
                GlobalState.TerrainPlacementSize = 3;
            }
        }

        private void Tile_MouseEnter(object sender, MouseEventArgs e)
        {
            var tile = sender as Label;
            if (tile != GlobalState.SelectedTile)
            {
                var border = tile.Content as Border;
                var grid = border.Child as Grid;
                var color = grid.Children[1] as Rectangle;
                color.Opacity = 1;
            }

        }

        private void Tile_MouseLeave(object sender, MouseEventArgs e)
        {
            var tile = sender as Label;
            if (tile != GlobalState.SelectedTile)
            {
                var border = tile.Content as Border;
                var grid = border.Child as Grid;
                var color = grid.Children[1] as Rectangle;
                color.Opacity = 0;
            }
        }

        private void Tile_MouseDown(object sender, MouseEventArgs e)
        {
            var tile = sender as Label;
            var border = tile.Content as Border;
            var grid = border.Child as Grid;
            var color = grid.Children[1] as Rectangle;
            if (GlobalState.SelectedTile != null)
            {
                var oldBorder = GlobalState.SelectedTile.Content as Border;
                var oldGrid = oldBorder.Child as Grid;
                var oldColor = oldGrid.Children[1] as Rectangle;
                oldBorder.BorderThickness = new Thickness(0);
                oldColor.Opacity = 0;
            }

            GlobalState.SelectedTile = tile;
            border.BorderThickness = new Thickness(2);
            color.Opacity = .5;
        }
    }
}
