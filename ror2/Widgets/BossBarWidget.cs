using System;
using System.Collections.Generic;
using RoR2.UI;
using UnityEngine;

namespace HideHud.Widgets
{
    // boss bar, the controller toggles its own container so fade the controller and read its ui fields
    internal sealed class BossBarWidget : PolledWidget
    {
        private HUDBossHealthBarController controller;

        public BossBarWidget(HudContext ctx) : base(ctx, PluginConfig.BossBar) { }

        protected override void ResolveRoots(List<Transform> roots)
        {
            controller = null;
            if (Hud.TryGetComponent(out ChildLocator locator) && locator.TryFindChild("BossHealthBar", out Transform fill) && fill)
                controller = fill.GetComponentInParent<HUDBossHealthBarController>();
            if (!controller)
            {
                Transform cluster = HudBinder.FindCluster(Hud, "TopCenterCluster");
                controller = cluster ? cluster.GetComponentInChildren<HUDBossHealthBarController>(true) : null;
            }
            if (!controller)
                controller = Hud.GetComponentInChildren<HUDBossHealthBarController>(true);
            if (controller)
                roots.Add(controller.transform);
        }

        private bool BossActive => controller && controller.container && controller.container.activeInHierarchy;

        protected override bool TrySample(out int hash)
        {
            hash = 0;
            if (!controller)
                return false;
            bool active = BossActive;
            int fill = 0;
            int label = 0;
            int name = 0;
            if (active)
            {
                if (controller.fillRectImage)
                {
                    fill = Mathf.RoundToInt(controller.fillRectImage.fillAmount * 1000f);
                    fill = fill * 31 + Mathf.RoundToInt(controller.fillRectImage.rectTransform.anchorMax.x * 1000f);
                }
                if (controller.healthLabel)
                    label = controller.healthLabel.text?.GetHashCode() ?? 0;
                if (controller.bossNameLabel)
                    name = controller.bossNameLabel.text?.GetHashCode() ?? 0;
            }
            hash = HashCode.Combine(active, fill, label, name);
            return true;
        }

        protected override bool CanHide()
        {
            return !(PluginConfig.KeepBossBarWhileActive.Value && BossActive);
        }
    }
}
