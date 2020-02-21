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

        public void DashSpeedUnitMetric_Click(object sender, RoutedEventArgs e)
        {
            Plugin.Settings.DashUnitMetric = (bool)DashUnitMetric.IsChecked;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            OverSpeedMargin.Value = Plugin.Settings.OverSpeedMargin;
            DashUnitMetric.IsChecked = Plugin.Settings.DashUnitMetric;
            WearWarning.Value = Plugin.Settings.WearWarningLevel;
        }
    }
}
