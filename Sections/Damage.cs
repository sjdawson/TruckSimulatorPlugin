namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Damage
    {
        private readonly TruckSimulatorPlugin Base;

        private float WearAverage;

        public Damage(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Damage.WearWarning", false);
            Base.AddProp("Damage.WearAverage", 0);

            Base.AddEvent("DamageIncrease");
        }

        public void DataUpdate()
        {
            var WearAverageCalculationValue = WearAverageCalculation();

            if (WearAverageCalculationValue > WearAverage) Base.TriggerEvent("DamageIncrease");

            WearAverage = WearAverageCalculationValue;
                       
            Base.SetProp("Damage.WearAverage", WearAverageCalculationValue);
            Base.SetProp("Damage.WearWarning", WearAverageCalculationValue > Base.Settings.WearWarningLevel);
        }

        /// <summary>
        /// The average of wear across all connected parts of the truck and trailer.
        /// </summary>
        private float WearAverageCalculation()
        {
            var totalWear = (float)Base.GetProp("Damage.WearCabin")
             + (float)Base.GetProp("Damage.WearChassis")
             + (float)Base.GetProp("Damage.WearEngine")
             + (float)Base.GetProp("Damage.WearTrailer")
             + (float)Base.GetProp("Damage.WearTransmission")
             + (float)Base.GetProp("Damage.WearWheels");

            return (totalWear / 6) * 100;
        }
    }
}
