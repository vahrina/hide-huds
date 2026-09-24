using RoR2.UI;
using UnityEngine;

namespace HideHud
{
    // version label isnt part of the hud and respawns on scene change so just poll it
    internal static class VersionLabelHider
    {
        private const float PollInterval = 1f;

        private static float pollTimer;

        public static void Tick()
        {
            pollTimer -= Time.unscaledDeltaTime;
            if (pollTimer > 0f)
                return;
            pollTimer = PollInterval;
            Apply();
        }

        public static void Apply()
        {
            bool visible = !(PluginConfig.Enabled.Value && PluginConfig.Version.Value);
            foreach (SteamBuildIdLabel label in Object.FindObjectsOfType<SteamBuildIdLabel>(true))
            {
                if (label.gameObject.activeSelf != visible)
                    label.gameObject.SetActive(visible);
            }
        }
    }
}
