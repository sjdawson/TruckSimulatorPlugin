using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Localisation
    {
        private readonly TruckSimulatorPlugin Base;
        private Dictionary<string, TruckSimulatorPluginCity> Cities;

        public Localisation(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            LoadCityLocalisations();

            // Localised versions of these strings
            Base.AddProp("L.Job.CitySource", "");
            Base.AddProp("L.Job.CountrySource", "");
            Base.AddProp("L.Job.CityDestination", "");
            Base.AddProp("L.Job.CountryDestination", "");

            // ASCII safe versions of these strings
            Base.AddProp("L.A.Job.CitySourceFromSDK", "");
            Base.AddProp("L.A.Job.CityDestinationFromSDK", "");

            // Localised, ASCII safe versions of these strings
            Base.AddProp("L.A.Job.CitySource", "");
            Base.AddProp("L.A.Job.CityDestination", "");
            Base.AddProp("L.A.Job.CountrySource", "");
            Base.AddProp("L.A.Job.CountryDestination", "");
        }

        public void DataUpdate()
        {
            if (Base.PluginManager.GameName == "ETS2")
            {
                Cities.TryGetValue((string)Base.GetProp("Job.CitySource"), out TruckSimulatorPluginCity CitySource);
                Cities.TryGetValue((string)Base.GetProp("Job.CityDestination"), out TruckSimulatorPluginCity CityDestination);

                if (CitySource != null)
                {
                    Base.SetProp("L.Job.CitySource", CitySource.translation);
                    Base.SetProp("L.Job.CountrySource", CitySource.country_translation);
                    Base.SetProp("L.A.Job.CitySourceFromSDK", CitySource.api_ascii);
                    Base.SetProp("L.A.Job.CitySource", CitySource.translation_ascii);
                    Base.SetProp("L.A.Job.CountrySource", CitySource.country_translation_ascii);
                }

                if (CityDestination != null)
                {
                    Base.SetProp("L.Job.CityDestination", CityDestination.translation);
                    Base.SetProp("L.Job.CountryDestination", CityDestination.country_translation);
                    Base.SetProp("L.A.Job.CityDestinationFromSDK", CityDestination.api_ascii);
                    Base.SetProp("L.A.Job.CityDestination", CityDestination.translation_ascii);
                    Base.SetProp("L.A.Job.CountryDestination", CityDestination.country_translation_ascii);
                }
            }
        }

        /// <summary>
        /// Loads the city localisation file into memory, which can be updated via the settings page
        /// </summary>
        public void LoadCityLocalisations()
        {
            if (Base.PluginManager.GameName == "ETS2")
            {
                string LangFile = File.ReadAllText(
                    Base.PluginManager.GetGameStoragePath() + "/sjdawson.TruckSimulatorPlugin.Translations/" + Base.Settings.LocalisationLanguage + ".json"
                );

                Cities = JsonConvert.DeserializeObject<Dictionary<string, TruckSimulatorPluginCity>>(LangFile);

                SimHub.Logging.Current.Info(String.Format(
                    "Loaded \"{0}\" localisations into memory",
                    Base.Settings.LocalisationLanguage
                ));
            }
        }

    }
}
