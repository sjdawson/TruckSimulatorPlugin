using GameReaderCommon;
using SimHub.Plugins;
using System;

namespace sjdawson.TruckSimulatorPlugin
{
    [PluginName("Truck Simulator Plugin")]
    [PluginDescription("Additional properties, actions and events for use in ETS2 and ATS.")]
    [PluginAuthor("sjdawson")]

    public class TruckSimulatorPlugin: IPlugin, IDataPlugin, IWPFSettings
    {
        public TruckSimulatorPluginSettings Settings;
        public PluginManager PluginManager { get; set; }

        public Sections.Damage Damage;
        public Sections.Dashboard Dashboard;
        public Sections.Drivetrain Drivetrain;
        public Sections.Engine Engine;
        public Sections.Job Job;
        public Sections.JobStatus JobStatus;
        public Sections.Lights Lights;
        public Sections.Localisation Localisation;
        public Sections.Navigation Navigation;

        /// <summary>
        /// Initialise the plugin preparing all settings, properties, events and triggers.
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            Settings = this.ReadCommonSettings<TruckSimulatorPluginSettings>("TruckSimulatorPluginSettings", () => new TruckSimulatorPluginSettings());

            Damage = new Sections.Damage(this);
            Dashboard = new Sections.Dashboard(this);
            Drivetrain = new Sections.Drivetrain(this);
            Engine = new Sections.Engine(this);
            Job = new Sections.Job(this);
            JobStatus = new Sections.JobStatus(this);
            Lights = new Sections.Lights(this);
            Localisation = new Sections.Localisation(this);
            Navigation = new Sections.Navigation(this);
        }

        /// <param name="pluginManager"></param>
        /// <param name="data"></param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            // We always want to be updating these properties, as they're game agnostic.
            Dashboard.DataUpdate();

            if (data.GameRunning && data.GameName == "ETS2" || data.GameName == "ATS")
            {
                if (data.OldData != null && data.NewData != null)
                {
                    Damage.DataUpdate();
                    Drivetrain.DataUpdate(ref data);
                    Engine.DataUpdate(ref data);
                    JobStatus.DataUpdate();
                    Job.DataUpdate();
                    Lights.DataUpdate();
                    Navigation.DataUpdate();

                    // These items are currently specific to ETS2 and won't work in ATS
                    if (data.GameName == "ETS2")
                    {
                        Localisation.DataUpdate();
                    }
                }
            }
        }

        public void End(PluginManager pluginManager) => this.SaveCommonSettings("TruckSimulatorPluginSettings", Settings);
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager) => new TruckSimulatorPluginSettingsControl(this);

        /// <summary>
        /// Calculate input as a percentage of the range min->max.
        /// </summary>
        /// <param name="input">The value to convert to a percentage</param>
        /// <param name="min">The minumum value where input would be equivalent 0%</param>
        /// <param name="max">The maximum value where input would be equivalent 100%</param>
        /// <returns>Float between 0-1 to represent 0-100%</returns>
        public float InputAsPercentageOfRange(float input, float min, float max) => input > min && input < max ? (input - min) / (max - min) : input > max ? 1 : 0;

        public void AddProp(string PropertyName, dynamic defaultValue) => PluginManager.AddProperty(PropertyName, GetType(), defaultValue);
        public void SetProp(string PropertyName, dynamic value) => PluginManager.SetPropertyValue(PropertyName, GetType(), value);
        public dynamic GetProp(string PropertyName) => PluginManager.GetPropertyValue("DataCorePlugin.GameRawData." + PropertyName);
        public bool HasProp(string PropertyName) => PluginManager.GetAllPropertiesNames().Contains("DataCorePlugin.GameRawData." + PropertyName);

        public void AddEvent(string EventName) => PluginManager.AddEvent(EventName, GetType());
        public void TriggerEvent(string EventName) => PluginManager.TriggerEvent(EventName, GetType());

        public void AddAction(string ActionName, Action<PluginManager, string> ActionBody) => PluginManager.AddAction(ActionName, GetType(), ActionBody);
    }
}
