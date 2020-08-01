using GameReaderCommon;

namespace sjdawson.TruckSimulatorPlugin.Sections
{
    public class Engine
    {
        private readonly TruckSimulatorPlugin Base;

        public Engine(TruckSimulatorPlugin truckSimulatorPlugin)
        {
            Base = truckSimulatorPlugin;

            Base.AddProp("Engine.Starting", false);
        }

        public void DataUpdate(ref GameData data)
        {
            Base.SetProp("Engine.Starting", ((bool)Base.GetProp("Drivetrain.EngineEnabled")) == false && data.NewData.Rpms > 0);
        }
    }
}
