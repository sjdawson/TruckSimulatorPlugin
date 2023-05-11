using System;

namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Navigation
    {
        private readonly TruckSimulatorPlugin Base;

        public Navigation(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Navigation.TotalDaysLeft", 0);
            Base.AddProp("Navigation.TotalHoursLeft", 0);
            Base.AddProp("Navigation.Minutes", 0);
        }

        public void DataUpdate()
        {
            var NavigationTime = (TimeSpan)Base.GetProp("NavigationValues.NavigationTime");
            Base.SetProp("Navigation.TotalDaysLeft", NavigationTime.Days);
            Base.SetProp("Navigation.TotalHoursLeft", NavigationTime.Hours);
            Base.SetProp("Navigation.Minutes", NavigationTime.Minutes);
        }
    }
}
