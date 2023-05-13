using GameReaderCommon;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Drivetrain
    {
        private readonly TruckSimulatorPlugin Base;

        private float FuelRangeStableValue;
        private float FuelAverageConsumption;

        // Only trucks with custom torque curves are set here, otherwise the fall to the 1000->1300 default the game uses.
        private Dictionary<string, ArrayList> PeakTorqueMap = new Dictionary<string, ArrayList>()
        {
            { "vehicle.man.tgx_euro6", new ArrayList() { 1000, 1400 } },
            { "vehicle.mercedes.actros", new ArrayList() { 1000, 1400 } },
            { "vehicle.mercedes.actros2014", new ArrayList() { 1000, 1200 } },
            { "vehicle.renault.t", new ArrayList() { 1000, 1400 } },
            { "vehicle.scania.r_2016", new ArrayList () { 1000, 1300 } },
        };

        private int PeakTorqueMinDefault = 1000;
        private int PeakTorqueMaxDefault = 1300;

        public Drivetrain(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Drivetrain.PeakTorque", false);
            Base.AddProp("Drivetrain.PeakTorque.Min", 0);
            Base.AddProp("Drivetrain.PeakTorque.Max", 0);

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
                        
            PeakTorqueMap.TryGetValue((string)Base.GetProp("TruckValues.ConstantsValues.Id"), out ArrayList PeakTorqueValues);

            if (PeakTorqueValues == null)
            {
                Base.SetProp("Drivetrain.PeakTorque", IsPeakTorque(data.NewData.Rpms, PeakTorqueMinDefault, PeakTorqueMaxDefault));
                Base.SetProp("Drivetrain.PeakTorque.Min", PeakTorqueMinDefault);
                Base.SetProp("Drivetrain.PeakTorque.Max", PeakTorqueMaxDefault);
            }
            else
            {
                Base.SetProp("Drivetrain.PeakTorque", IsPeakTorque(data.NewData.Rpms, (int)PeakTorqueValues[0], (int)PeakTorqueValues[1]));
                Base.SetProp("Drivetrain.PeakTorque.Min", (int)PeakTorqueValues[0]);
                Base.SetProp("Drivetrain.PeakTorque.Max", (int)PeakTorqueValues[1]);
            }

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
            var FuelRangeCurrentValue = (float)Base.GetProp("TruckValues.CurrentValues.DashboardValues.FuelValue.Range");

            FuelRangeStableValue = FuelRangeCurrentValue > 0
                ? FuelRangeCurrentValue
                : FuelRangeStableValue;

            return FuelRangeStableValue;
        }

        /// <summary>
        /// Are you currently within the peak torque range of the truck's RPM? Given
        /// the limited data returned by the SDK, the best we can do for this
        /// attribute is base it on reported statistics of each base truck, and
        /// not their upgrades.
        /// </summary>
        private bool IsPeakTorque(double CurrentRpm, int PeakTorqueMin, int PeakTorqueMax)
        {
            return CurrentRpm >= PeakTorqueMin && CurrentRpm <= PeakTorqueMax;
        }

        /// <summary>
        /// Provides indication of being in a crawler gear when the gearing
        /// has 14 total forward gears. It's a guesstimate at best, but since
        /// the data isn't directly provided by the SDK, it's the best we can
        /// do for now!
        /// </summary>
        private string DrivetrainGearDashboardWithCrawler()
        {
            var GearDashboard = (int)Base.GetProp("TruckValues.CurrentValues.DashboardValues.GearDashboards");
            var GearsForward = (int)Base.GetProp("TruckValues.ConstantsValues.MotorValues.ForwardGearCount");

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
