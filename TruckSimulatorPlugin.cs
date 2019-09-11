using GameReaderCommon;
using SimHub.Plugins;
using System;

namespace sjdawson.TruckSimulatorPlugin
{
    [PluginDescription("Additional properties, actions and events for use in truck simulators, ETS2 and ATS.")]
    [PluginAuthor("sjdawson")]
    [PluginName("Truck Simulator Plugin")]

    public class TruckSimulatorPlugin: IPlugin, IDataPlugin, IWPFSettings
    {
        public TruckSimulatorPluginSettings Settings;
        public PluginManager PluginManager { get; set; }

        /// <param name="pluginManager"></param>
        /// <param name="data"></param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (data.GameRunning && data.GameName == "ETS2" || data.GameName == "ATS")
            {
                if (data.OldData != null && data.NewData != null)
                {
                    pluginManager.SetPropertyValue("Job.NextRestWarning", this.GetType(), this.NextRestWarning(pluginManager));
                    pluginManager.SetPropertyValue("Job.OverSpeedLimit", this.GetType(), this.OverSpeedLimit(pluginManager, Settings.OverSpeedMargin));
                    pluginManager.SetPropertyValue("Job.OverSpeedLimitPercentage", this.GetType(), this.OverSpeedLimitPercentage(pluginManager, Settings.OverSpeedMargin));

                    pluginManager.SetPropertyValue("Navigation.TotalDaysLeft", this.GetType(), this.NavigationDaysLeft(pluginManager));
                    pluginManager.SetPropertyValue("Navigation.TotalHoursLeft", this.GetType(), this.NavigationHoursLeft(pluginManager));
                    pluginManager.SetPropertyValue("Navigation.Minutes", this.GetType(), this.NavigationMinutes(pluginManager));

                    pluginManager.SetPropertyValue("Drivetrain.EcoRange", this.GetType(), this.EcoRange(data.NewData.Rpms));

                    float wearAverage = this.WearAverage(pluginManager);
                    pluginManager.SetPropertyValue("Damage.WearAverage", this.GetType(), wearAverage);
                    pluginManager.SetPropertyValue("Damage.WearWarning", this.GetType(), wearAverage > 5);

                    pluginManager.SetPropertyValue("Lights.HazardWarningActive", this.GetType(), this.HazardWarningLightsActive(pluginManager));
                }
            }
        }

        /// <summary>
        /// Indicates whether you're currently speeding considering the current limit set on the road.
        /// Only works when the limit is greater than zero.
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="overSpeedMargin"></param>
        /// <returns>bool</returns>
        public bool OverSpeedLimit(PluginManager pluginManager, int overSpeedMargin)
        {
            float speedLimit = (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Job.SpeedLimitMph");
            float currentSpeed = (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Drivetrain.SpeedMph");

            return speedLimit > 0 && currentSpeed > (speedLimit + overSpeedMargin);
        }

        /// <summary>
        /// When you're over the speed limit, this will return a percentage value indicating
        /// how far over you are, taking current speed limit plus over speed margin as 100% over.
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns>float</returns>
        public float OverSpeedLimitPercentage(PluginManager pluginManager, int overSpeedMargin)
        {
            float speedLimit = (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Job.SpeedLimitMph");
            float currentSpeed = (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Drivetrain.SpeedMph");

            if (speedLimit > 0)
            {
                return this.InputAsPercentageOfRange(currentSpeed, speedLimit, speedLimit + overSpeedMargin);
            }

            return 0;
        }

        /// <summary>
        /// Are you currently within the econimical range of the truck's RPM?
        /// </summary>
        /// <param name="rpms"></param>
        /// <returns>bool</returns>
        public bool EcoRange(double rpms)
        {
            /// This may need tweaking, based on different trucks.
            return rpms > 1000 && rpms < 1800;
        }

        /// <summary>
        /// Return the total number of days remaining in navigation time
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns>int</returns>
        public int NavigationDaysLeft(PluginManager pluginManager)
        {
            TimeSpan timeLeft = (TimeSpan)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Job.NavigationTime");

            return (int)timeLeft.TotalDays;
        }

        /// <summary>
        /// Return the total number of hours remaining in navigation time
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns>int</returns>
        public int NavigationHoursLeft(PluginManager pluginManager)
        { 
            TimeSpan timeLeft = (TimeSpan)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Job.NavigationTime");

            return (int)timeLeft.TotalHours;
        }

        /// <summary>
        /// Return the minutes component from navigation time
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns>int</returns>
        public int NavigationMinutes(PluginManager pluginManager)
        { 
            TimeSpan timeLeft = (TimeSpan)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Job.NavigationTime");

            return (int)timeLeft.Minutes;
        }

        /// <summary>
        /// The average of the wear across all connected parts of the truck.
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns>float</returns>
        public float WearAverage(PluginManager pluginManager)
        {
            float totalWear = (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Damage.WearCabin")
                + (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Damage.WearChassis")
                + (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Damage.WearEngine")
                + (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Damage.WearTrailer")
                + (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Damage.WearTransmission")
                + (float)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Damage.WearWheels");

            return (totalWear / 6) * 100;
        }

        public DateTime HazardLightsOnAt;
        public bool HazardLightsOn = false;

        /// <summary>
        /// If both blinkers are 'on', then the hazards are on. This is a shortcut
        /// to having to check both properties manually.
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns>bool</returns>
        public bool HazardWarningLightsActive(PluginManager pluginManager)
        {
            DateTime now = DateTime.Now;

            bool switchedOn = (bool)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Lights.BlinkerLeftOn") &&
                (bool)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.Lights.BlinkerRightOn");

            if (switchedOn)
            {
                this.HazardLightsOnAt = now;
                this.HazardLightsOn = true;
            }
            
            // If the lights are off longer than 1s after being "on", they've been turned off
            if (switchedOn == false && now > this.HazardLightsOnAt.AddSeconds(1))
            {
                this.HazardLightsOn = false;
            }           

            return this.HazardLightsOn;
        }

        /// <summary>
        /// Indicates whether you have less than an "hour" remaining before you need to rest.
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns>bool</returns>
        public bool NextRestWarning(PluginManager pluginManager)
        {
            TimeSpan nextRest = (TimeSpan)pluginManager.GetPropertyValue("DataCorePlugin.GameRawData.NextRestStopTime");

            return nextRest.Hours < 1;
        }

        /// <summary>
        /// Calculate the percentage of `input` based against `min` and `max` values
        /// being 0-1 respectively.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private float InputAsPercentageOfRange(float input, float min, float max)
        {
            if (input > min && input < max)
            {
                return ((input - min) / (max - min));
            }

            return input > max ? 1 : 0;
        }

        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            this.SaveCommonSettings("TruckSimulatorPluginSettings", Settings);
        }

        /// <param name="pluginManager"></param>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            /// @todo Make the settings configurable
            return new TruckSimulatorPluginSettingsControl(this);
        }

        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            Settings = this.ReadCommonSettings<TruckSimulatorPluginSettings>("TruckSimulatorPluginSettings", () => new TruckSimulatorPluginSettings());

            // Additional job information
            pluginManager.AddProperty("Job.NextRestWarning", this.GetType(), false);
            pluginManager.AddProperty("Job.OverSpeedLimit", this.GetType(), false);
            pluginManager.AddProperty("Job.OverSpeedLimitPercentage", this.GetType(), 0);
            
            // Additional navigation information
            pluginManager.AddProperty("Navigation.TotalDaysLeft", this.GetType(), false);
            pluginManager.AddProperty("Navigation.TotalHoursLeft", this.GetType(), false);
            pluginManager.AddProperty("Navigation.Minutes", this.GetType(), false);

            // Additional truck information
            pluginManager.AddProperty("Drivetrain.EcoRange", this.GetType(), false);
            pluginManager.AddProperty("Damage.WearAverage", this.GetType(), 0);
            pluginManager.AddProperty("Damage.WearWarning", this.GetType(), false);
            pluginManager.AddProperty("Lights.HazardWarningActive", this.GetType(), false);

            // Plugin information
            pluginManager.AddProperty("Dash.DisplayUnitMetric", this.GetType(), false);            

            pluginManager.AddAction("SwitchDisplayUnit", this.GetType(), (a, b) =>
            {
                pluginManager.SetPropertyValue("Dash.DisplayUnitMetric", this.GetType(),
                    !(bool)pluginManager.GetPropertyValue("TruckSimulatorPlugin.Dash.DisplayUnitMetric")
                );
            });
        }
    }
}
