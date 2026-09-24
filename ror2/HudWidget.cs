using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine;

namespace HideHud
{
    // one piece of the hud that hides by itself, like hunkhud's DisplayMover
    // activity resets the timer, fades out when it runs out and CanHide() says ok
    internal abstract class HudWidget
    {
        protected readonly HudContext Ctx;
        private readonly ConfigEntry<bool> enabled;
        private float activeTimer;

        public readonly List<Transform> Roots = new List<Transform>();
        public float Alpha { get; private set; } = 1f;
        public bool IsBound { get; private set; }

        protected HudWidget(HudContext ctx, ConfigEntry<bool> enabled)
        {
            Ctx = ctx;
            this.enabled = enabled;
        }

        protected HUD Hud => Ctx.Hud;
        public bool IsEnabled => enabled.Value;

        // true when roots dont match the config anymore and need a rebind
        public virtual bool BindingStale => false;

        protected abstract void ResolveRoots(List<Transform> roots);
        protected abstract bool CheckForActivity();
        protected virtual bool CanHide() => true;

        // hud target changed, forget old values so the switch doesnt count as activity
        public virtual void OnTargetChanged() { }

        protected virtual void OnUnbind() { }

        public bool TryBind()
        {
            Roots.Clear();
            ResolveRoots(Roots);
            Roots.RemoveAll(r => !r);
            if (Roots.Count == 0)
                return false;
            IsBound = true;
            OnTargetChanged();
            SetActive();
            return true;
        }

        public void Unbind()
        {
            if (!IsBound)
                return;
            IsBound = false;
            Roots.Clear();
            Alpha = 1f;
            OnUnbind();
        }

        public void SetActive()
        {
            activeTimer = PluginConfig.IdleSeconds.Value;
        }

        public void Tick(float deltaTime, bool forceVisible)
        {
            if (!enabled.Value)
            {
                Alpha = 1f;
                return;
            }

            if (CheckForActivity())
                SetActive();

            if (CanHide())
                activeTimer -= deltaTime;
            else
                activeTimer = Mathf.Max(activeTimer, PluginConfig.IdleSeconds.Value);

            bool visible = forceVisible || activeTimer > 0f;
            float target = visible ? 1f : 0f;
            float duration = visible ? PluginConfig.FadeInSeconds.Value : PluginConfig.FadeOutSeconds.Value;
            Alpha = duration <= 0f ? target : Mathf.MoveTowards(Alpha, target, deltaTime / duration);
        }
    }

    // widget that hashes its state, any change from last time counts as activity
    internal abstract class PolledWidget : HudWidget
    {
        private int lastHash;
        private bool hasSample;

        protected PolledWidget(HudContext ctx, ConfigEntry<bool> enabled) : base(ctx, enabled) { }

        protected abstract bool TrySample(out int hash);

        protected override bool CheckForActivity()
        {
            if (!TrySample(out int hash))
            {
                hasSample = false;
                return false;
            }
            if (!hasSample)
            {
                hasSample = true;
                lastHash = hash;
                return false;
            }
            if (hash == lastHash)
                return false;
            lastHash = hash;
            return true;
        }

        public override void OnTargetChanged()
        {
            hasSample = false;
        }
    }
}
