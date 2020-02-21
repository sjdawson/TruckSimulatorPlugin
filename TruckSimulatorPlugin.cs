using GameReaderCommon;
using SimHub.Plugins;
using System;
using System.Collections.Generic;

namespace sjdawson.TruckSimulatorPlugin
{
    [PluginDescription("Additional properties, actions and events for use in truck simulators, ETS2 and ATS.")]
    [PluginAuthor("sjdawson")]
    [PluginName("Truck Simulator Plugin")]

    public class TruckSimulatorPlugin: IPlugin, IDataPlugin, IWPFSettings
    {
        public TruckSimulatorPluginSettings Settings;
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Initialise the plugin preparing all settings, properties, events and triggers.
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            Settings = this.ReadCommonSettings<TruckSimulatorPluginSettings>("TruckSimulatorPluginSettings", () => new TruckSimulatorPluginSettings());

            // Additional properties relating to job attributes
            AddProp("Job.InProgress", false);
            AddProp("Job.NextRestWarning", false);
            AddProp("Job.OverSpeedLimit", false);
            AddProp("Job.OverSpeedLimitPercentage", 0);
            AddProp("Job.TotalDaysLeft", 0);
            AddProp("Job.TotalHoursLeft", 0);
            AddProp("Job.Minutes", 0);

            // Additional properties relating to navigation attributes
            AddProp("Navigation.TotalDaysLeft", 0);
            AddProp("Navigation.TotalHoursLeft", 0);
            AddProp("Navigation.Minutes", 0);

            // Additional properties relating to vehicle attributes
            AddProp("Drivetrain.EcoRange", false);
            AddProp("Damage.WearAverage", 0);

            AddProp("Damage.WearWarning", false);

            AddProp("Lights.HazardWarningOn", false);
            AddProp("Engine.Starting", false);

            // Additional properties relating to global attributes
            AddProp("Dash.DisplayUnitMetric", false);

            // Additional actions that can be triggered via input
            pluginManager.AddAction("SwitchDisplayUnit", this.GetType(), (a, b) =>
            {
                Settings.DashUnitMetric = !(bool)Settings.DashUnitMetric;
            });

            // Additional events triggered via attribues in this plugin
            AddEvent("JobStarted");
            AddEvent("JobCompleted");
            AddEvent("DamageIncrease");
        }

        /// <param name="pluginManager"></param>
        /// <param name="data"></param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (data.GameRunning && data.GameName == "ETS2" || data.GameName == "ATS")
            {
                if (data.OldData != null && data.NewData != null)
                {
                    SetProp("Job.InProgress", JobInProgress());
                    SetProp("Job.NextRestWarning", NextRestWarning());
                    SetProp("Job.OverSpeedLimit", OverSpeedLimit());
                    SetProp("Job.OverSpeedLimitPercentage", OverSpeedLimitPercentage());
                    SetProp("Job.TotalDaysLeft", TotalDays("DataCorePlugin.GameRawData.Job.RemainingTime"));
                    SetProp("Job.TotalHoursLeft", TotalHours("DataCorePlugin.GameRawData.Job.RemainingTime"));
                    SetProp("Job.Minutes", Minutes("DataCorePlugin.GameRawData.Job.RemainingTime"));

                    SetProp("Navigation.TotalDaysLeft", TotalDays("DataCorePlugin.GameRawData.Job.NavigationTime"));
                    SetProp("Navigation.TotalHoursLeft", TotalHours("DataCorePlugin.GameRawData.Job.NavigationTime"));
                    SetProp("Navigation.Minutes", Minutes("DataCorePlugin.GameRawData.Job.NavigationTime"));

                    SetProp("Drivetrain.EcoRange", EcoRange(data.NewData.Rpms));

                    float WearAverage = WearAverageCalculation();
                    if (WearAverage > 0 && WearAverage > (float)GetProp("TruckSimulatorPlugin.Damage.WearAverage"))
                    {
                        PluginManager.TriggerEvent("DamageIncrease", GetType());
                    }
                    SetProp("Damage.WearAverage", WearAverage);
                    SetProp("Damage.WearWarning", WearAverage.CompareTo(Settings.WearWarningLevel) > 0);

                    SetProp("Lights.HazardWarningOn", HazardWarningOn());

                    SetProp("Engine.Starting", ((bool)GetProp("DataCorePlugin.GameRawData.Drivetrain.EngineEnabled")) == false && data.NewData.Rpms > 0);

                    // Loop all of the latches as they match property name, and set their values.
                    foreach (var Data in Latches)
                    {
                        SetProp(Data.Key, TestLatch(Data.Key));
                    }
                }
            }

            SetProp("Dash.DisplayUnitMetric", Settings.DashUnitMetric);
        }

        public void End(PluginManager pluginManager) => this.SaveCommonSettings("TruckSimulatorPluginSettings", Settings);
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager) => new TruckSimulatorPluginSettingsControl(this);

        private Dictionary<string, DateTime> Latches = new Dictionary<string, DateTime>();

        private string CurrentJobString = "";
        private bool JobActive = false;
        private bool SpeedLimitSeen = false;
        private bool ZeroNavAndDistanceAtSet = false;
        private DateTime ZeroNavAndDistanceAt;

        /// <summary>
        /// Indicates whether you're currently working on a job or not.
        /// </summary>
        private bool JobInProgress()
        {
            string CurrentJob = String.Format("{0}__{1}__{2}__{3}__{4}",
                (string)GetProp("DataCorePlugin.GameRawData.Job.Cargo"),
                (string)GetProp("DataCorePlugin.GameRawData.Job.CompanySource"),
                (string)GetProp("DataCorePlugin.GameRawData.Job.CitySource"),
                (string)GetProp("DataCorePlugin.GameRawData.Job.CompanyDestination"),
                (string)GetProp("DataCorePlugin.GameRawData.Job.CityDestination")
            ).Replace(" ", "-").Replace("________", "").ToLower();

            if (CurrentJob == "")
            {
                JobActive = false;
            }

            if (CurrentJobString != CurrentJob)
            {
                CurrentJobString = CurrentJob;
                JobActive = true;
                SpeedLimitSeen = false;
                ZeroNavAndDistanceAtSet = false;
                ZeroNavAndDistanceAt = DateTime.Now.AddYears(1); // Force the date out in the future so it'd never be true at job start

                PluginManager.TriggerEvent("JobStarted", GetType());
            }

            if (JobActive)
            {
                float speedLimit = (float)GetProp("DataCorePlugin.GameRawData.Job.SpeedLimit");
                float navigationDistanceAndTimeLeft = (float)GetProp("DataCorePlugin.GameRawData.Job.NavigationDistanceLeft")
                    + (float)GetProp("DataCorePlugin.GameRawData.Job.NavigationTimeLeft");

                if (speedLimit > 0 && !SpeedLimitSeen)
                {
                    SpeedLimitSeen = true;
                }

                if (navigationDistanceAndTimeLeft.Equals(0)
                    && SpeedLimitSeen
                )
                {
                    ZeroNavAndDistanceAt = DateTime.Now.AddSeconds(3);
                    ZeroNavAndDistanceAtSet = true;
                }

                if (navigationDistanceAndTimeLeft.Equals(0)
                    && ZeroNavAndDistanceAtSet
                    && DateTime.Now.CompareTo(ZeroNavAndDistanceAt) > 0
                )
                {
                    JobActive = false;
                    SpeedLimitSeen = false;
                    ZeroNavAndDistanceAtSet = false;
                    ZeroNavAndDistanceAt = DateTime.Now.AddYears(1);

                    PluginManager.TriggerEvent("JobCompleted", GetType());
                }
                else if (navigationDistanceAndTimeLeft.CompareTo(0) > 0
                    && ZeroNavAndDistanceAtSet
                )
                {
                    ZeroNavAndDistanceAt = DateTime.Now.AddYears(1);
                    ZeroNavAndDistanceAtSet = false;
                }
            }

            return JobActive;
        }

        /// <summary>
        /// Indicates whether you're currently speeding considering the current limit set on the road.
        /// Only works when the limit is greater than zero.
        /// </summary>
        /// <returns>bool</returns>
        private bool OverSpeedLimit()
        {
            float speedLimit = (float)GetProp("DataCorePlugin.GameRawData.Job.SpeedLimitMph");
            float currentSpeed = (float)GetProp("DataCorePlugin.GameRawData.Drivetrain.SpeedMph");

            return speedLimit > 0 && currentSpeed > (speedLimit + Settings.OverSpeedMargin);
        }

        /// <summary>
        /// When you're over the speed limit, this will return a percentage value indicating
        /// how far over you are, taking current speed limit + over speed margin as 100% over.
        /// </summary>
        private float OverSpeedLimitPercentage()
        {
            float speedLimit = (float)GetProp("DataCorePlugin.GameRawData.Job.SpeedLimitMph");
            float currentSpeed = (float)GetProp("DataCorePlugin.GameRawData.Drivetrain.SpeedMph");

            return speedLimit > 0
                ? InputAsPercentageOfRange(currentSpeed, speedLimit, speedLimit + Settings.OverSpeedMargin)
                : 0;
        }

        /// <summary>
        /// Are you currently within the eco range of the truck's RPM?
        /// </summary>
        /// <param name="Rpms">Current RPM of the vehicle</param>
        private bool EcoRange(double Rpms)
        {
            var minRpms = 1000;
            var maxRpms = 1400;

            switch ((string)GetProp("DataCorePlugin.GameRawData.TruckId")) {
                case "vehicle.volvo.fh16":
                    minRpms = 1000;
                    maxRpms = 1400;
                    break;
            }

            return Rpms > minRpms && Rpms < maxRpms;
        }

        /// <summary>
        /// The average of the wear across all connected parts of the truck.
        /// </summary>
        private float WearAverageCalculation()
        {
            var totalWear = (float)GetProp("DataCorePlugin.GameRawData.Damage.WearCabin")
             + (float)GetProp("DataCorePlugin.GameRawData.Damage.WearChassis")
             + (float)GetProp("DataCorePlugin.GameRawData.Damage.WearEngine")
             + (float)GetProp("DataCorePlugin.GameRawData.Damage.WearTrailer")
             + (float)GetProp("DataCorePlugin.GameRawData.Damage.WearTransmission")
             + (float)GetProp("DataCorePlugin.GameRawData.Damage.WearWheels");

            return (totalWear / 6) * 100;
        }

        private DateTime HazardLightsOnAt;
        private bool HazardLightsOn = false;

        /// <summary>
        /// If both blinkers are on, then the hazards are on. We have to use the debounce
        /// method, as BlinkerLeftActive / RightActive are false when hazards are on.
        /// </summary>
        /// <returns>True if both blinkers are flashing and haven't stopped for more than a set time</returns>
        private bool HazardWarningOn()
        {
            var now = DateTime.Now;
            var blinkerLeftAndRightOn = (bool)GetProp("DataCorePlugin.GameRawData.Lights.BlinkerLeftOn")
                && (bool)GetProp("DataCorePlugin.GameRawData.Lights.BlinkerRightOn");

            if (blinkerLeftAndRightOn)
            {
                HazardLightsOnAt = now;
                HazardLightsOn = true;
            }

            // We need both lights off for at least 1s to consider the hazards as off
            if (!blinkerLeftAndRightOn && now > HazardLightsOnAt.AddSeconds(1))
            {
                HazardLightsOn = false;
            }

            return HazardLightsOn;
        }

        /// <summary>
        /// Indicates whether you have less than an hour remaining before you need to rest.
        /// </summary>
        /// <returns>True if 1 hour or less remains</returns>
        private bool NextRestWarning() => ((TimeSpan)GetProp("DataCorePlugin.GameRawData.NextRestStopTime")).Hours < 1;

        /// <summary>
        /// Calculate input as a percentage of the range min->max.
        /// </summary>
        /// <param name="input">The value to convert to a percentage</param>
        /// <param name="min">The minumum value where input would be equivalent 0%</param>
        /// <param name="max">The maximum value where input would be equivalent 100%</param>
        /// <returns>Float between 0-1 to represent 0-100%</returns>
        private float InputAsPercentageOfRange(float input, float min, float max) => input > min && input < max ? (input - min) / (max - min) : input > max ? 1 : 0;

        /// <summary>
        /// Test the value of an entry in the Latches dict, to see if it has expired or not
        /// </summary>
        /// <param name="LatchName">The relevant key from the Latches dict to test</param>
        /// <returns>True if the latch hasn't outlived its expiry date</returns>
        private bool TestLatch(string LatchName) => Latches.TryGetValue(LatchName, out DateTime Expires) && DateTime.Now < Expires;

        private void AddProp(string PropertyName, bool defaultValue) => PluginManager.AddProperty(PropertyName, GetType(), defaultValue);
        private void AddProp(string PropertyName, int defaultValue) => PluginManager.AddProperty(PropertyName, GetType(), defaultValue);
        private void AddProp(string PropertyName, string defaultValue) => PluginManager.AddProperty(PropertyName, GetType(), defaultValue);
        private void AddProp(string PropertyName, TimeSpan defaultValue) => PluginManager.AddProperty(PropertyName, GetType(), defaultValue);
        private void AddLatchProps(string BasePropertyName, int[] intervals)
        {
            for (int i = 0; i < intervals.Length; i++)
            {
                AddProp(BasePropertyName + intervals[i].ToString() + "s", false);
            }
        }

        private void SetLatch(string LatchName, DateTime ExpirationTime) => Latches[LatchName] = ExpirationTime;
        private void SetLatchProps(string BasePropertyName, int[] intervals)
        {
            for (int i = 0; i < intervals.Length; i++)
            {
                SetLatch(BasePropertyName + intervals[i].ToString() + "s", DateTime.Now.AddSeconds(intervals[i]));
            }
        }

        private void AddEvent(string EventName) => PluginManager.AddEvent(EventName, GetType());

        private void SetProp(string PropertyName, bool value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);
        private void SetProp(string PropertyName, float value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);
        private void SetProp(string PropertyName, int value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);
        private void SetProp(string PropertyName, string value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);
        private void SetProp(string PropertyName, TimeSpan value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);

        private object GetProp(string PropertyName) => PluginManager.GetPropertyValue(PropertyName);

        private int TotalDays(string PropertyName) => (int)((TimeSpan)GetProp(PropertyName)).TotalDays;
        private int TotalHours(string PropertyName) => (int)((TimeSpan)GetProp(PropertyName)).TotalHours;
        private int Minutes(string PropertyName) => (int)((TimeSpan)GetProp(PropertyName)).Minutes;
    }
}
