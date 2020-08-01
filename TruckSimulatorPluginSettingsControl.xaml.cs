using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace sjdawson.TruckSimulatorPlugin
{
    /// <summary>
    /// Interaction logic for TruckSimulatorPluginSettingsControl.xaml
    /// </summary>
    public partial class TruckSimulatorPluginSettingsControl : UserControl
    {
        public TruckSimulatorPlugin Plugin { get; }

        public TruckSimulatorPluginSettingsControl()
        {
            InitializeComponent();

            var LocalisationLanguages = new List<LocalisationLanguageChoice>() {
                new LocalisationLanguageChoice("Basque (Spain)", "eu_es"),
                new LocalisationLanguageChoice("Bulgarian", "bg_bg"),
                new LocalisationLanguageChoice("Catalan (Spain)", "ca_es"),
                new LocalisationLanguageChoice("Chinese (S)", "zh_cn"),
                new LocalisationLanguageChoice("Chinese (T)", "zh_tw"),
                new LocalisationLanguageChoice("Croatian", "hr_hr"),
                new LocalisationLanguageChoice("Czech", "cs_cz"),
                new LocalisationLanguageChoice("Danish", "da_dk"),
                new LocalisationLanguageChoice("Dutch", "nl_nl"),
                new LocalisationLanguageChoice("English (United Kingdom)", "en_gb"),
                new LocalisationLanguageChoice("English (United States)", "en_us"),
                new LocalisationLanguageChoice("Estonian", "et_ee"),
                new LocalisationLanguageChoice("Finnish", "fi_fi"),
                new LocalisationLanguageChoice("French", "fr_fr"),
                new LocalisationLanguageChoice("FYRO Macedonian", "mk_mk"),
                new LocalisationLanguageChoice("Galician (Spain)", "gl_es"),
                new LocalisationLanguageChoice("Georgian", "ka_ge"),
                new LocalisationLanguageChoice("German", "de_de"),
                new LocalisationLanguageChoice("Greek", "el_gr"),
                new LocalisationLanguageChoice("Hungarian", "hu_hu"),
                new LocalisationLanguageChoice("Italian", "it_it"),
                new LocalisationLanguageChoice("Japanese", "ja_jp"),
                new LocalisationLanguageChoice("Korean", "ko_kr"),
                new LocalisationLanguageChoice("Latvian", "lv_lv"),
                new LocalisationLanguageChoice("Lithuanian", "lt_lt"),
                new LocalisationLanguageChoice("Norwegian", "no_no"),
                new LocalisationLanguageChoice("Polish", "pl_pl"),
                new LocalisationLanguageChoice("Portuguese (Brazil)", "pt_br"),
                new LocalisationLanguageChoice("Portuguese (Portugal)", "pt_pt"),
                new LocalisationLanguageChoice("Romanian", "ro_ro"),
                new LocalisationLanguageChoice("Russian", "ru_ru"),
                new LocalisationLanguageChoice("Serbian", "sr_sp"),
                new LocalisationLanguageChoice("Slovak", "sk_sk"),
                new LocalisationLanguageChoice("Slovenian", "sl_sl"),
                new LocalisationLanguageChoice("Spanish", "es_es"),
                new LocalisationLanguageChoice("Swedish", "sv_se"),
                new LocalisationLanguageChoice("Turkish", "tr_tr"),
                new LocalisationLanguageChoice("Ukrainian", "uk_uk"),
                new LocalisationLanguageChoice("Vietnamese", "vi_vn"),
            };

            LocalisationLanguage.DataContext = LocalisationLanguages;
        }

        public TruckSimulatorPluginSettingsControl(TruckSimulatorPlugin plugin) : this()
        {
            this.Plugin = plugin;
        }

        public void OverSpeedMarginChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            Plugin.Settings.OverSpeedMargin = (int)OverSpeedMargin.Value;
        }

        public void WearWarningChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            Plugin.Settings.WearWarningLevel = (int)WearWarning.Value;
        }

        public void LocalisationLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            Plugin.Settings.LocalisationLanguage = (string)LocalisationLanguage.SelectedValue;
            Plugin.Localisation.LoadCityLocalisations();
        }

        public void DashSpeedUnitMetric_Click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.DashUnitMetric = (bool)DashUnitMetric.IsChecked;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            OverSpeedMargin.Value = Plugin.Settings.OverSpeedMargin;
            DashUnitMetric.IsChecked = Plugin.Settings.DashUnitMetric;
            WearWarning.Value = Plugin.Settings.WearWarningLevel;
            LocalisationLanguage.SelectedValue = Plugin.Settings.LocalisationLanguage;
        }
    }

    public class LocalisationLanguageChoice
    {
        public LocalisationLanguageChoice(string display, string value)
        {
            Display = display;
            Value = value;
        }

        public string Value { get; set; }
        public string Display { get; set; }
    }
}
