using System;

namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Job
    {
        private readonly TruckSimulatorPlugin Base;

        public Job(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Job.OverSpeedLimit", false);
            Base.AddProp("Job.OverSpeedLimitPercentage", 0);

            Base.AddProp("Job.NextRestWarning", false);
            Base.AddProp("Job.RemainingDeliveryTime.Time.Days", 0);
            Base.AddProp("Job.RemainingDeliveryTime.Time.Hours", 0);
            Base.AddProp("Job.RemainingDeliveryTime.Time.Minutes", 0);
        }

        public void DataUpdate()
        {
            Base.SetProp("Job.OverSpeedLimit", OverSpeedLimit());
            Base.SetProp("Job.OverSpeedLimitPercentage", OverSpeedLimitPercentage());
                        
            Base.SetProp("Job.NextRestWarning", ((TimeSpan)Base.GetProp("NextRestStopTime")).Hours < 1);

            var RemainingTime = (TimeSpan)Base.GetProp("JobValues.RemainingDeliveryTime.Time");
            Base.SetProp("Job.RemainingDeliveryTime.Time.Days", RemainingTime.Days);
            Base.SetProp("Job.RemainingDeliveryTime.Time.Hours", RemainingTime.Hours);
            Base.SetProp("Job.RemainingDeliveryTime.Time.Minutes", RemainingTime.Minutes);
        }

        /// <summary>
        /// Indicates whether you're currently speeding considering the current limit set on the road.
        /// Only works when the limit is greater than zero.
        /// </summary>
        private bool OverSpeedLimit()
        {
            float speedLimit = (float)Base.GetProp("Job.SpeedLimitMph");
            float currentSpeed = (float)Base.GetProp("Drivetrain.SpeedMph");

            return speedLimit > 0 && currentSpeed > (speedLimit + Base.Settings.OverSpeedMargin);
        }

        /// <summary>
        /// When you're over the speed limit, this will return a percentage value indicating
        /// how far over you are, taking current speed limit + over speed margin as 100% over.
        /// </summary>
        private float OverSpeedLimitPercentage()
        {
            float SpeedLimit = (float)Base.GetProp("Job.SpeedLimitMph");
            float CurrentSpeed = (float)Base.GetProp("Drivetrain.SpeedMph");

            return SpeedLimit > 0
                ? Base.InputAsPercentageOfRange(CurrentSpeed, SpeedLimit, SpeedLimit + Base.Settings.OverSpeedMargin)
                : 0;
        }
    }
}
