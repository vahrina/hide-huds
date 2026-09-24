using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace HideHud.Widgets
{
    // stays up while health or shield isnt full, barrier and overcharge dont count
    internal sealed class HealthWidget : HudWidget
    {
        private const float ChangeEpsilon = 0.0005f;

        private bool hasSample;
        private float health, shield, barrier, curse, osp;
        private bool voidShields;

        public HealthWidget(HudContext ctx) : base(ctx, PluginConfig.Health) { }

        protected override void ResolveRoots(List<Transform> roots)
        {
            if (Hud.healthBar)
                roots.Add(Hud.healthBar.transform);
        }

        public override void OnTargetChanged()
        {
            hasSample = false;
        }

        protected override bool CheckForActivity()
        {
            HealthComponent hc = Ctx.Health;
            if (!hc)
            {
                hasSample = false;
                return false;
            }

            HealthComponent.HealthBarValues values = hc.GetHealthBarValues();
            float h = hc.health;
            float s = hc.shield;
            float b = hc.barrier;
            float c = values.curseFraction;
            float o = values.ospFraction;
            bool v = values.hasVoidShields;

            bool changed = false;
            if (hasSample)
            {
                float full = Mathf.Max(1f, hc.fullCombinedHealth);
                float eps = full * ChangeEpsilon;
                // barrier decays every tick, only count gains or drops bigger than decay
                float barrierDropTolerance = Mathf.Max(hc.fullBarrier * 0.01f, eps);
                changed = Mathf.Abs(h - health) > eps
                    || Mathf.Abs(s - shield) > eps
                    || b - barrier > eps
                    || barrier - b > barrierDropTolerance
                    || Mathf.Abs(c - curse) > ChangeEpsilon
                    || Mathf.Abs(o - osp) > ChangeEpsilon
                    || v != voidShields;
            }

            health = h;
            shield = s;
            barrier = b;
            curse = c;
            osp = o;
            voidShields = v;
            hasSample = true;
            return changed;
        }

        protected override bool CanHide()
        {
            HealthComponent hc = Ctx.Health;
            if (!hc || !hc.alive)
                return true;
            float max = hc.fullHealth + hc.fullShield;
            float tolerance = Mathf.Max(0.5f, max * 0.001f);
            return hc.health + hc.shield >= max - tolerance;
        }
    }
}
