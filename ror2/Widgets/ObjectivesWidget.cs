using System.Collections.Generic;
using RoR2.UI;
using TMPro;
using UnityEngine;

namespace HideHud.Widgets
{
    // objective panel, checks the text a few times a second
    internal sealed class ObjectivesWidget : PolledWidget
    {
        private const float SampleInterval = 0.2f;

        private readonly List<TMP_Text> texts = new List<TMP_Text>();
        private ObjectivePanelController panel;
        private float sampleTimer;
        private int cachedHash;

        public ObjectivesWidget(HudContext ctx) : base(ctx, PluginConfig.Objectives) { }

        public static ObjectivePanelController FindPanel(HUD hud)
        {
            GameObject gameModeUi = hud.gameModeUiInstance;
            ObjectivePanelController found = gameModeUi ? gameModeUi.GetComponentInChildren<ObjectivePanelController>(true) : null;
            return found ? found : hud.GetComponentInChildren<ObjectivePanelController>(true);
        }

        protected override void ResolveRoots(List<Transform> roots)
        {
            panel = FindPanel(Hud);
            if (panel)
                roots.Add(panel.transform);
        }

        public override void OnTargetChanged()
        {
            base.OnTargetChanged();
            sampleTimer = 0f;
        }

        protected override bool TrySample(out int hash)
        {
            hash = cachedHash;
            if (!panel)
                return false;

            sampleTimer -= Time.unscaledDeltaTime;
            if (sampleTimer > 0f)
                return true;
            sampleTimer = SampleInterval;

            panel.GetComponentsInChildren(false, texts);
            int h = 17;
            foreach (TMP_Text text in texts)
            {
                if (text && text.enabled)
                    h = h * 31 + (text.text?.GetHashCode() ?? 0);
            }
            h = h * 31 + texts.Count;
            texts.Clear();
            cachedHash = h;
            hash = h;
            return true;
        }
    }
}
