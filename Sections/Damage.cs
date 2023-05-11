using System.Linq;

namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Damage
    {
        private readonly TruckSimulatorPlugin Base;

        private float Average;

        public Damage(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Damage.WearWarning", false);
            Base.AddProp("Damage.WearAverage", 0);

            Base.AddEvent("DamageIncrease");
        }

        public void DataUpdate()
        {
            var AverageCalculationValue = WearAverageCalculation();

            // Trigger the event if we take a greater hit than 1% of damage
            if (AverageCalculationValue > (Average + 1)) Base.TriggerEvent("DamageIncrease");

            Average = AverageCalculationValue;

            Base.SetProp("Damage.WearWarning", AverageCalculationValue > Base.Settings.WearWarningLevel);
            Base.SetProp("Damage.WearAverage", AverageCalculationValue);
        }

        /// <summary>
        /// The average damage across all connected parts of the truck.
        /// </summary>
        private float WearAverageCalculation()
        {
            float[] totalWear = {
                (float)Base.GetProp("TruckValues.CurrentValues.DamageValues.Cabin"),
                (float)Base.GetProp("TruckValues.CurrentValues.DamageValues.Chassis"),
                (float)Base.GetProp("TruckValues.CurrentValues.DamageValues.Engine"),
                (float)Base.GetProp("TruckValues.CurrentValues.DamageValues.Transmission"),
                (float)Base.GetProp("TruckValues.CurrentValues.DamageValues.WheelsAvg"),
            };

            return totalWear.Sum() / totalWear.Length * 100;
        }
    }
}
