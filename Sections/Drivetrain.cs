using GameReaderCommon;
using System;

namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Drivetrain
    {
        private readonly TruckSimulatorPlugin Base;

        private float FuelRangeStableValue;
        private float FuelAverageConsumption;

        public Drivetrain(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Drivetrain.EcoRange", false);
            Base.AddProp("Drivetrain.FuelRangeStable", 0);
            Base.AddProp("Drivetrain.FuelValue.AverageConsumptionLitresPer100Mile", 0);
            Base.AddProp("Drivetrain.FuelValue.AverageConsumptionMilesPerGallonUK", 0);
            Base.AddProp("Drivetrain.FuelValue.AverageConsumptionMilesPerGallonUS", 0);
            Base.AddProp("Drivetrain.GearDashboard", 0);
        }

        public void DataUpdate(ref GameData data)
        {
            var FuelAverageConsumptionCurrentValue = (float)Base.GetProp("TruckValues.CurrentValues.DashboardValues.FuelValue.AverageConsumption");

            FuelAverageConsumption = FuelAverageConsumptionCurrentValue > 0
                ? FuelAverageConsumptionCurrentValue
                : FuelAverageConsumption;

            Base.SetProp("Drivetrain.EcoRange", EcoRange(data.NewData.Rpms));
            Base.SetProp("Drivetrain.FuelRangeStable", FuelRangeStable());
            Base.SetProp("Drivetrain.FuelValue.AverageConsumptionLitresPer100Mile", FuelAverageConsumption * (float)160.9344);
            Base.SetProp("Drivetrain.FuelValue.AverageConsumptionMilesPerGallonUK", FuelAverageConsumption * (float)2.824809363);
            Base.SetProp("Drivetrain.FuelValue.AverageConsumptionMilesPerGallonUS", FuelAverageConsumption * (float)2.352145833);
            Base.SetProp("Drivetrain.GearDashboard", DrivetrainGearDashboardWithCrawler());
        }

        /// <summary>
        /// Maintains your fuel range indication to avoid dips to 0 constantly.
        /// </summary>
        private float FuelRangeStable()
        {
            var FuelRangeCurrentValue = (float)Base.GetProp("Drivetrain.FuelRange");

            FuelRangeStableValue = FuelRangeCurrentValue > 0
                ? FuelRangeCurrentValue
                : FuelRangeStableValue;

            return FuelRangeStableValue;
        }

        /// <summary>
        /// Are you currently within the eco range of the truck's RPM? Given
        /// the limited data returned by the SDK, the best we can do for this
        /// attribute is base it on reported statistics of each base truck, and
        /// not their upgrades.
        /// </summary>
        private bool EcoRange(double CurrentRpm)
        {
            var minRpm = 1000;
            var maxRpm = 1400;

            switch ((string)Base.GetProp("TruckId"))
            {
                case "vehicle.volvo.fh16":
                    minRpm = 1000;
                    maxRpm = 1400;
                    break;
            }

            return CurrentRpm >= minRpm && CurrentRpm <= maxRpm;
        }

        /// <summary>
        /// Provides indication of being in a crawler gear when the gearing
        /// has 14 total forward gears. It's a guesstimate at best, but since
        /// the data isn't directly provided by the SDK, it's the best we can
        /// do for now!
        /// </summary>
        private string DrivetrainGearDashboardWithCrawler()
        {
            var GearDashboard = (int)Base.GetProp("Drivetrain.GearDashboard");
            var GearsForward = (int)Base.GetProp("Drivetrain.GearsForward");

            if (GearsForward == 14) // 12+2 transmission
            {
                if (GearDashboard == 1 || GearDashboard == 2)
                {
                    return String.Format("C{0}", GearDashboard);
                }

                if (GearDashboard > 2)
                {
                    return (GearDashboard - 2).ToString();
                }
            }

            if (GearDashboard < 0)
            {
                return String.Format("R{0}", Math.Abs(GearDashboard));
            }

            if (GearDashboard == 0)
            {
                return "N";
            }

            return GearDashboard.ToString();
        }
    }
}
