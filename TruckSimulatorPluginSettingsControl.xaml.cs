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
            OverSpeedMargin.Value = 5;
        }

        public TruckSimulatorPluginSettingsControl(TruckSimulatorPlugin plugin) : this()
        {
            this.Plugin = plugin;
        }

        public void OverSpeedMarginChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            this.Plugin.PluginManager.TriggerAction("Modify");
        }
    }
}
