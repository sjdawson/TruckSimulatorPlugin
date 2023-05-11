using SimHub.Plugins;
using System;

namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Lights
    {
        private readonly TruckSimulatorPlugin Base;

        private bool AreHazardLightsOn;
        private DateTime HazardLightsOnAt;

        public Lights(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Lights.HazardWarningOn", false);
        }

        public void DataUpdate()
        {
            Base.SetProp("Lights.HazardWarningOn", HazardWarningOn());
        }

        /// <summary>
        /// If both blinkers are on, then we can consider the hazard lights as on, as you can't indicate
        /// both ways in normal circumstances.
        /// </summary>
        private bool HazardWarningOn()
        {
            var now = DateTime.Now;
            var bothBlinkersAreOn = (bool)Base.GetProp("TruckValues.CurrentValues.LightsValues.BlinkerLeftOn") && (bool)Base.GetProp("TruckValues.CurrentValues.LightsValues.BlinkerRightOn");

            if (bothBlinkersAreOn)
            {
                HazardLightsOnAt = now;
                AreHazardLightsOn = true;
            }

            if (!bothBlinkersAreOn && now > HazardLightsOnAt.AddSeconds(1))
            {
                AreHazardLightsOn = false;
            }

            return AreHazardLightsOn;
        }
    }
}
