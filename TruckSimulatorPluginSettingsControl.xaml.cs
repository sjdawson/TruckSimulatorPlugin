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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            OverSpeedMargin.Value = Plugin.Settings.OverSpeedMargin;
        }
    }
}
