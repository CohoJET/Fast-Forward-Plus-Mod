using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;

namespace FastForwardPlus
{
    /// <summary>
    /// Binds a key to each of a configurable list of simulation speeds, so speeds past vanilla's 3x
    /// are reachable without touching the timeback menu's two-state button.
    /// </summary>
    [BepInPlugin(PluginInfo.PluginGuid, PluginInfo.PluginName, PluginInfo.PluginVersion)]
    public class FastForwardPlusPlugin : BasePlugin
    {
        internal static ManualLogSource Logger;

        public override void Load()
        {
            Logger = Log;
            Log.LogInfo($"{PluginInfo.PluginName} {PluginInfo.PluginVersion} loaded.");

            SpeedBindings.Load(Config);

            AddComponent<SpeedHotkeys>();
            AddComponent<SpeedLabel>();
        }
    }
}
