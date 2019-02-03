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
                result.Append(Location_Name_Prefix.Text);
                if (Location_Name_CountingType.SelectedItem == Location_Name_CountingType_Numeric_NoLead)
                {
                    result.Append(GlobalState.Locations.IndexOf(location) + 1);
                }
                else if (Location_Name_CountingType.SelectedItem == Location_Name_CountingType_Numeric_Lead)
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
                else if (Location_Name_CountingType.SelectedItem == Location_Name_CountingType_Alphabetic_Upper)
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
                else if (Location_Name_CountingType.SelectedItem == Location_Name_CountingType_Alphabetic_Lower)
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

        private void Location_Naming_Changed(object sender, EventArgs e)
        {
            var s = new StringBuilder();
            s.Append(Location_Name_Prefix.Text);
            if (Location_Name_CountingType.SelectedItem == Location_Name_CountingType_Numeric_NoLead)
            {
                s.Append("1");
            }
            else if (Location_Name_CountingType.SelectedItem == Location_Name_CountingType_Numeric_Lead)
            {
                s.Append("01");
            }
            else if (Location_Name_CountingType.SelectedItem == Location_Name_CountingType_Alphabetic_Upper)
            {
                s.Append("A");
            }
            else if (Location_Name_CountingType.SelectedItem == Location_Name_CountingType_Alphabetic_Lower)
            {
                s.Append("a");
            }

            Location_Name_Preview.Content = s.ToString();
            SetLocationNames();
        }
    }
}
