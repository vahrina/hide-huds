using BepInEx;
using BepInEx.Logging;

namespace HideHud
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.bepis.bepinex.configurationmanager", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginAuthor = "vah";
        public const string PluginName = "HideHud";
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
        public const string PluginVersion = "0.1.2";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            PluginConfig.Bind(Config);
        }

        private void OnEnable()
        {
            HudBinder.Enable();
        }

        private void Update()
        {
            VersionLabelHider.Tick();
        }

        private void OnDisable()
        {
            HudBinder.Disable();
        }
    }
}
