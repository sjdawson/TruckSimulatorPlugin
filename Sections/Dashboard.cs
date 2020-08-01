namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Dashboard
    {
        private readonly TruckSimulatorPlugin Base;

        public Dashboard(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Dashboard.DisplayUnitMetric", false);

            Base.AddAction("SwitchDisplayUnit", (a, b) =>
            {
                Base.Settings.DashUnitMetric = !Base.Settings.DashUnitMetric;
            });
        }

        public void DataUpdate()
        {
            Base.SetProp("Dashboard.DisplayUnitMetric", Base.Settings.DashUnitMetric);
        }
    }
}
