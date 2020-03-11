using GameReaderCommon;
using SimHub.Plugins;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace sjdawson.TruckSimulatorPlugin
{
    [PluginDescription("Additional properties, actions and events for use in truck simulators, ETS2 and ATS.")]
    [PluginAuthor("sjdawson")]
    [PluginName("Truck Simulator Plugin")]

    public class TruckSimulatorPlugin: IPlugin, IDataPlugin, IWPFSettings
    {
        public TruckSimulatorPluginSettings Settings;
        public PluginManager PluginManager { get; set; }
        private Dictionary<string, TruckSimulatorPluginCity> Cities;

        /// <summary>
        /// Initialise the plugin preparing all settings, properties, events and triggers.
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            Settings = this.ReadCommonSettings<TruckSimulatorPluginSettings>("TruckSimulatorPluginSettings", () => new TruckSimulatorPluginSettings());

            LoadCityLocalisations();

            // JOB
            AddProp("Job.InProgress", false);
            AddProp("Job.Status", false);
            AddProp("Job.NextRestWarning", false);
            AddProp("Job.OverSpeedLimit", false);
            AddProp("Job.OverSpeedLimitPercentage", 0);
            AddProp("Job.TotalDaysLeft", 0);
            AddProp("Job.TotalHoursLeft", 0);
            AddProp("Job.Minutes", 0);

            // Localised versions of these strings
            AddProp("Job.L10N.CitySource", "");
            AddProp("Job.L10N.CityDestination", "");
            AddProp("Job.L10N.CountrySource", "");
            AddProp("Job.L10N.CountryDestination", "");
            
            // ASCII safe versions of these strings
            AddProp("Job.ASCII.CitySource", "");
            AddProp("Job.ASCII.CityDestination", "");

            // Localised, ASCII safe versions of these strings
            AddProp("Job.ASCII.L10N.CitySource", "");
            AddProp("Job.ASCII.L10N.CityDestination", "");
            AddProp("Job.ASCII.L10N.CountrySource", "");
            AddProp("Job.ASCII.L10N.CountryDestination", "");

            // NAVIGATION
            AddProp("Navigation.TotalDaysLeft", 0);
            AddProp("Navigation.TotalHoursLeft", 0);
            AddProp("Navigation.Minutes", 0);

            // VEHICLE
            AddProp("Drivetrain.EcoRange", false);
            AddProp("Drivetrain.FuelRangeStable", 0);
            AddProp("Drivetrain.GearDashboard", 0);
            AddProp("Damage.WearAverage", 0);
            AddProp("Damage.WearWarning", false);
            AddProp("Lights.HazardWarningOn", false);
            AddProp("Engine.Starting", false);

            // PLUGIN
            AddProp("Dash.DisplayUnitMetric", false);

            // ACTIONS
            PluginManager.AddAction("SwitchDisplayUnit", this.GetType(), (a, b) =>
            {
                Settings.DashUnitMetric = !(bool)Settings.DashUnitMetric;
            });

            PluginManager.AddAction("ResetJobStatus", GetType(), (a, b) =>
            {
                JobStatus = "none";
                CurrentJobHash = "";
                HasSeenSpeedLimit = false;
                HasBeenCloseToDestination = false;
                HasNavDistanceZeroSet = false;
                JobStatusLatch = DateTime.Now;
                NavDistanceZeroAt = DateTime.Now;
                PluginManager.TriggerEvent("JobReset", GetType());
            });

            // EVENTS
            AddEvent("JobTaken");
            AddEvent("JobLoading");
            AddEvent("JobOngoing");
            AddEvent("JobCompleted");
            AddEvent("JobAbandoned");
            AddEvent("JobReset");
            AddEvent("DamageIncrease");
        }

        private float WearAverage = 0;

        /// <param name="pluginManager"></param>
        /// <param name="data"></param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (data.GameRunning && data.GameName == "ETS2" || data.GameName == "ATS")
            {
                if (data.OldData != null && data.NewData != null)
                {
                    SetProp("Job.NextRestWarning", NextRestWarning());
                    SetProp("Job.OverSpeedLimit", OverSpeedLimit());
                    SetProp("Job.OverSpeedLimitPercentage", OverSpeedLimitPercentage());
                    SetProp("Job.TotalDaysLeft", (int)((TimeSpan)GetProp("DataCorePlugin.GameRawData.Job.RemainingTime")).TotalDays);
                    SetProp("Job.TotalHoursLeft", (int)((TimeSpan)GetProp("DataCorePlugin.GameRawData.Job.RemainingTime")).TotalHours);
                    SetProp("Job.Minutes", ((TimeSpan)GetProp("DataCorePlugin.GameRawData.Job.RemainingTime")).Minutes);

                    Cities.TryGetValue((string)GetProp("DataCorePlugin.GameRawData.Job.CitySource"), out TruckSimulatorPluginCity CitySource);
                    Cities.TryGetValue((string)GetProp("DataCorePlugin.GameRawData.Job.CityDestination"), out TruckSimulatorPluginCity CityDestination);

                    if (CitySource != null)
                    {
                        SetProp("Job.L10N.CitySource", CitySource.translation);
                        SetProp("Job.L10N.CountrySource", CitySource.country_translation);
                        SetProp("Job.ASCII.CitySource", CitySource.api_ascii);
                        SetProp("Job.ASCII.L10N.CitySource", CitySource.translation_ascii);
                        SetProp("Job.ASCII.L10N.CountrySource", CitySource.country_translation_ascii);
                    }

                    if (CityDestination != null)
                    {
                        SetProp("Job.L10N.CityDestination", CityDestination.translation);
                        SetProp("Job.L10N.CountryDestination", CityDestination.country_translation);
                        SetProp("Job.ASCII.CityDestination", CityDestination.api_ascii);
                        SetProp("Job.ASCII.L10N.CityDestination", CityDestination.translation_ascii);
                        SetProp("Job.ASCII.L10N.CountryDestination", CityDestination.country_translation_ascii);
                    }

                    SetProp("Navigation.TotalDaysLeft", (int)((TimeSpan)GetProp("DataCorePlugin.GameRawData.Job.NavigationTime")).TotalDays);
                    SetProp("Navigation.TotalHoursLeft", (int)((TimeSpan)GetProp("DataCorePlugin.GameRawData.Job.NavigationTime")).TotalHours);
                    SetProp("Navigation.Minutes", ((TimeSpan)GetProp("DataCorePlugin.GameRawData.Job.NavigationTime")).Minutes);

                    SetProp("Drivetrain.EcoRange", EcoRange(data.NewData.Rpms));
                    SetProp("Drivetrain.FuelRangeStable", FuelRangeStable());
                    SetProp("Drivetrain.GearDashboard", DrivetrainGearDashboardWithCrawler());

                    float WearAverageCalculationValue = WearAverageCalculation();
                    if (WearAverageCalculationValue > WearAverage)
                    {
                        PluginManager.TriggerEvent("DamageIncrease", GetType());
                    }
                    WearAverage = WearAverageCalculationValue;
                    SetProp("Damage.WearAverage", WearAverageCalculationValue);
                    SetProp("Damage.WearWarning", WearAverageCalculationValue > Settings.WearWarningLevel);

                    SetProp("Lights.HazardWarningOn", HazardWarningOn());

                    SetProp("Engine.Starting", ((bool)GetProp("DataCorePlugin.GameRawData.Drivetrain.EngineEnabled")) == false && data.NewData.Rpms > 0);

                    JobInProgress();
                    SetProp("Job.Status", JobStatus);
                    SetProp("Job.InProgress", JobStatus == "taken" || JobStatus == "loading" || JobStatus == "ongoing");

                    if (NavDistancePrevFrames.Count >= 20)
                    {
                        NavDistancePrevFrames.RemoveAt(0);
                    }

                    NavDistancePrevFrames.Add(NavDistancePrevFrame);
                    NavDistancePrevFrame = (float)GetProp("DataCorePlugin.GameRawData.Job.NavigationDistanceLeft");

                    // Less than five 0 frames is a blip, and shouldn't count
                    if (NavDistancePrevFrames.FindAll(x => x == 0).Count < 5)
                    {
                        NavDistancePrevFrames.RemoveAll(x => x == 0);
                    }

                    if (NavDistancePrevFrames.Count > 0)
                    {
                        HasNavJumpedInDistance = NavDistancePrevFrames.Max() - NavDistancePrevFrames.Min() > 300;
                    }
                }
            }

            SetProp("Dash.DisplayUnitMetric", Settings.DashUnitMetric);
        }

        public void End(PluginManager pluginManager) => this.SaveCommonSettings("TruckSimulatorPluginSettings", Settings);
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager) => new TruckSimulatorPluginSettingsControl(this);

        private string DrivetrainGearDashboardWithCrawler()
        {
            var CurrentGear = (int)GetProp("DataCorePlugin.GameRawData.Drivetrain.GearDashboard");
            var GearsForward = (int)GetProp("DataCorePlugin.GameRawData.Drivetrain.GearsForward");

            if (GearsForward == 14) // 12+2 transmission
            {
                if (CurrentGear == 1 || CurrentGear == 2)
                {
                    return String.Format("C{0}", CurrentGear);
                }

                if (CurrentGear > 2)
                {
                    return (CurrentGear - 2).ToString();
                }
            }

            if (CurrentGear < 0)
            {
                return String.Format("R{0}", Math.Abs(CurrentGear));
            }

            if (CurrentGear == 0)
            {
                return "N";
            }

            return CurrentGear.ToString();
        }

        private string JobStatus = "none"; // none, taken, loading, ongoing, completed, abandoned
        private string CurrentJobHash = "";
        private bool HasSeenSpeedLimit = false;
        private bool HasBeenCloseToDestination = false;
        private bool HasNavDistanceZeroSet = false;
        private bool HasNavJumpedInDistance = false;
        private DateTime JobStatusLatch = DateTime.Now;
        private DateTime NavDistanceZeroAt;
        private float NavDistancePrevFrame;
        private List<float> NavDistancePrevFrames = new List<float>() { 0 };

        private void JobInProgress()
        {
            var CurrentJob = String.Format("{0}__{1}__{2}__{3}__{4}",
                (string)GetProp("DataCorePlugin.GameRawData.Job.Cargo"),
                (string)GetProp("DataCorePlugin.GameRawData.Job.CompanySource"),
                (string)GetProp("DataCorePlugin.GameRawData.Job.CitySource"),
                (string)GetProp("DataCorePlugin.GameRawData.Job.CompanyDestination"),
                (string)GetProp("DataCorePlugin.GameRawData.Job.CityDestination")
            ).Replace(" ", "-").Replace("________", "").ToLower();

            // If current job is blank - or we don't have a trailer, we don't have a job
            if (CurrentJob == "")
            {
                JobStatus = "none";

                return;
            }

            // We can continue calculating properties if current job isn't blanked
            var NavDistanceLeft = (float)GetProp("DataCorePlugin.GameRawData.Job.NavigationDistanceLeft");

            if (NavDistanceLeft == 0 && HasNavDistanceZeroSet == false)
            {
                NavDistanceZeroAt = DateTime.Now.AddSeconds(2);
                HasNavDistanceZeroSet = true;
            }

            if (NavDistanceLeft > 0 && HasNavDistanceZeroSet == true)
            {
                NavDistanceZeroAt = DateTime.Now.AddYears(1);
                HasNavDistanceZeroSet = false;
            }
            var NavEnded = HasNavDistanceZeroSet && DateTime.Now > NavDistanceZeroAt;
            var SpeedLimit = (float)GetProp("DataCorePlugin.GameRawData.Job.SpeedLimit");

            // Job string has changed, we've taken a new job (Clicked "Take Job")
            if (JobStatus == "none" && CurrentJob != CurrentJobHash && DateTime.Now > JobStatusLatch)
            {
                JobStatus = "taken";
                PluginManager.TriggerEvent("JobTaken", GetType());

                JobStatusLatch = DateTime.Now.AddYears(1);
                CurrentJobHash = CurrentJob;
                HasSeenSpeedLimit = false;
                HasBeenCloseToDestination = false;

                return;
            }

            // Job taken, 0 distance, we're loading the cargo
            if (JobStatus == "taken" && NavEnded)
            {
                JobStatus = "loading";
                PluginManager.TriggerEvent("JobLoading", GetType());

                return;
            }

            // Job taken, distance is set, cargo is loaded (or skipped - quick job)
            if ((JobStatus == "taken" || JobStatus == "loading") && NavDistanceLeft > 0)
            {
                JobStatus = "ongoing";
                PluginManager.TriggerEvent("JobOngoing", GetType());

                return;
            }

            // Whilst job is ongoing ---
            if (JobStatus == "ongoing")
            {
                // And we've seen a speed limit
                if (SpeedLimit > 0 && HasSeenSpeedLimit == false)
                {
                    HasSeenSpeedLimit = true;
                }

                // And we've been close to destination
                if (NavDistanceLeft < 30 && NavDistanceLeft > 0 && HasSeenSpeedLimit && HasBeenCloseToDestination == false)
                {
                    HasBeenCloseToDestination = true;
                }

                // And we've had both flags set ---
                if (HasSeenSpeedLimit && HasBeenCloseToDestination)
                {
                    // And the job string has changed since last iteration
                    // Or the job string is the same, but returned to free drive or set a new GPS destination
                    if ((CurrentJob != CurrentJobHash)
                        || (CurrentJob == CurrentJobHash && (NavEnded || HasNavJumpedInDistance)))
                    {
                        JobStatus = "completed";
                        JobStatusLatch = DateTime.Now.AddSeconds(2);
                        PluginManager.TriggerEvent("JobCompleted", GetType());

                        return;
                    }   
                }
                else
                {
                    // If the job hash changes without having those flags set, then we've abandoned that job
                    // This won't update until the next job is taken, however
                    if (CurrentJob != CurrentJobHash)
                    {
                        JobStatus = "abandoned";
                        JobStatusLatch = DateTime.Now.AddSeconds(2);
                        PluginManager.TriggerEvent("JobAbandoned", GetType());

                        return;
                    }
                }
            }

            // Job has been completed or abandoned, and we've surpassed the 2 second latch timer for those statuses
            // - Perform a reset of all variables used in this method
            if ((JobStatus == "completed" || JobStatus == "abandoned") && DateTime.Now > JobStatusLatch)
            {
                JobStatus = "none";
                JobStatusLatch = DateTime.Now.AddSeconds(2);

                HasSeenSpeedLimit = false;
                HasBeenCloseToDestination = false;

                return;
            }
        }

        private float FuelRangeStableValue = 0;

        /// <summary>
        /// Maintains your fuel range indication to avoid dips to 0 constantly
        /// </summary>
        /// <returns>The current value of the property, or last known good if it's currently zero</returns>
        private float FuelRangeStable()
        {
            var FuelRangeCurrentValue = (float)GetProp("DataCorePlugin.GameRawData.Drivetrain.FuelRange");

            if (FuelRangeCurrentValue > 0)
            {
                FuelRangeStableValue = FuelRangeCurrentValue;
            }

            return FuelRangeStableValue;
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

        private void AddProp(string PropertyName, bool defaultValue) => PluginManager.AddProperty(PropertyName, GetType(), defaultValue);
        private void AddProp(string PropertyName, int defaultValue) => PluginManager.AddProperty(PropertyName, GetType(), defaultValue);
        private void AddProp(string PropertyName, string defaultValue) => PluginManager.AddProperty(PropertyName, GetType(), defaultValue);

        private void AddEvent(string EventName) => PluginManager.AddEvent(EventName, GetType());

        private void SetProp(string PropertyName, bool value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);
        private void SetProp(string PropertyName, float value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);
        private void SetProp(string PropertyName, int value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);
        private void SetProp(string PropertyName, string value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);

        private object GetProp(string PropertyName) => PluginManager.GetPropertyValue(PropertyName);

        public void LoadCityLocalisations()
        {
            string LangFile = File.ReadAllText(PluginManager.GetGameStoragePath() + "/sjdawson.TruckSimulatorPlugin.Translations/" + Settings.LocalisationLanguage + ".json");
            Cities = JsonConvert.DeserializeObject<Dictionary<string, TruckSimulatorPluginCity>>(LangFile);
            SimHub.Logging.Current.Info(String.Format("Loaded \"{0}\" localisations into memory", Settings.LocalisationLanguage));
        }
    }
}
