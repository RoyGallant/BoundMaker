using System;
using System.Text;
using System.Windows.Controls;

namespace BoundMaker.Views.Panels
{
    public partial class LocationPanel : UserControl
    {

        public LocationPanel()
        {
            InitializeComponent();
        }

        public void SetLocationNames()
        {
            foreach (Models.MapLocation location in GlobalState.Locations)
            {
                var result = new StringBuilder();
                result.Append(LocationNamePrefix.Text);
                if (LocationNameCountingType.SelectedItem == LocationNameCountingTypeNumericNoLead)
                {
                    result.Append(GlobalState.Locations.IndexOf(location) + 1);
                }
                else if (LocationNameCountingType.SelectedItem == LocationNameCountingTypeNumericLead)
                {
                    if (GlobalState.Locations.IndexOf(location) < 10 && GlobalState.Locations.Count >= 10)
                    {
                        result.Append("0");
                    }

                    if (GlobalState.Locations.IndexOf(location) < 100 && GlobalState.Locations.Count >= 100)
                    {
                        result.Append("0");
                    }

                    if (GlobalState.Locations.IndexOf(location) < 1000 && GlobalState.Locations.Count >= 1000)
                    {
                        result.Append("0");
                    }

                    result.Append(GlobalState.Locations.IndexOf(location) + 1);
                }
                else if (LocationNameCountingType.SelectedItem == LocationNameCountingTypeAlphabeticUpper)
                {
                    if (GlobalState.Locations.Count < 27)
                    {
                        result.Append((char)(65 + GlobalState.Locations.IndexOf(location)));
                    }
                    else
                    {
                        result.Append((char)(65 + GlobalState.Locations.IndexOf(location) / 26) + "" + (char)(65 + GlobalState.Locations.IndexOf(location) % 26));
                    }
                }
                else if (LocationNameCountingType.SelectedItem == LocationNameCountingTypeAlphabeticLower)
                {
                    if (GlobalState.Locations.Count < 27)
                    {
                        result.Append((char)(97 + GlobalState.Locations.IndexOf(location)));
                    }
                    else
                    {
                        result.Append((char)(97 + GlobalState.Locations.IndexOf(location) / 26) + "" + (char)(97 + GlobalState.Locations.IndexOf(location) % 26));
                    }
                }
                location.LocationName = result.ToString();
            }
        }

        private void LocationNamingChangedEventHandler(object sender, EventArgs e)
        {
            var s = new StringBuilder();
            s.Append(LocationNamePrefix.Text);
            if (LocationNameCountingType.SelectedItem == LocationNameCountingTypeNumericNoLead)
            {
                s.Append("1");
            }
            else if (LocationNameCountingType.SelectedItem == LocationNameCountingTypeNumericLead)
            {
                s.Append("01");
            }
            else if (LocationNameCountingType.SelectedItem == LocationNameCountingTypeAlphabeticUpper)
            {
                s.Append("A");
            }
            else if (LocationNameCountingType.SelectedItem == LocationNameCountingTypeAlphabeticLower)
            {
                s.Append("a");
            }

            LocationNamePreview.Content = s.ToString();
            SetLocationNames();
        }
    }
}
