using System.Collections.Generic;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine;

namespace HideHud
{
    [DisallowMultipleComponent]
    internal sealed class HudAutoHideController : MonoBehaviour
    {
        private const float BindInterval = 0.5f;

        private HUD hud;
        private HudContext ctx;
        private readonly HudFader fader = new HudFader();
        private readonly List<HudWidget> widgets = new List<HudWidget>();
        private float bindTimer;
        private bool running;

        private void Awake()
        {
            hud = GetComponent<HUD>();
            ctx = new HudContext(hud);
            widgets.Add(new Widgets.HealthWidget(ctx));
            widgets.Add(new Widgets.MoneyWidget(ctx));
            widgets.Add(new Widgets.ItemsWidget(ctx));
            widgets.Add(new Widgets.EquipmentWidget(ctx));
            widgets.Add(new Widgets.ExperienceWidget(ctx));
            widgets.Add(new Widgets.BuffsWidget(ctx));
            widgets.Add(new Widgets.SkillsWidget(ctx));
            widgets.Add(new Widgets.ObjectivesWidget(ctx));
            widgets.Add(new Widgets.AlliesWidget(ctx));
            widgets.Add(new Widgets.BossBarWidget(ctx));
            widgets.Add(new Widgets.DifficultyWidget(ctx));
        }

        private void OnDestroy()
        {
            Stop();
        }

        private void Update()
        {
            if (!hud)
                return;

            if (!PluginConfig.Enabled.Value)
            {
                if (running)
                    Stop();
                return;
            }
            running = true;

            float dt = Time.unscaledDeltaTime;

            bindTimer -= dt;
            if (bindTimer <= 0f)
            {
                bindTimer = BindInterval;
                BindWidgets();
            }

            if (ctx.Refresh())
            {
                foreach (HudWidget widget in widgets)
                {
                    widget.OnTargetChanged();
                    widget.SetActive();
                }
            }

            bool forceVisible = IsPeekHeld() || (PluginConfig.ShowAllOnScoreboard.Value && IsScoreboardOpen());
            foreach (HudWidget widget in widgets)
            {
                if (widget.IsBound)
                    widget.Tick(dt, forceVisible);
            }

            fader.Apply();
        }

        private void BindWidgets()
        {
            bool changed = false;
            foreach (HudWidget widget in widgets)
            {
                if (widget.IsBound && (widget.BindingStale || !fader.HasLiveGroup(widget)))
                {
                    fader.RemoveWidget(widget);
                    widget.Unbind();
                    changed = true;
                }
                if (!widget.IsBound && widget.TryBind())
                {
                    fader.AddWidget(widget);
                    changed = true;
                }
            }
            if (changed)
                AddSharedContainers();
        }

        // BarRoots and LevelDisplayCluster have their own backgrounds, they follow the most visible child
        private void AddSharedContainers()
        {
            if (hud.buffDisplay)
                AddSharedIfSafe(hud.buffDisplay.transform.parent);
            if (hud.healthBar)
                AddSharedIfSafe(hud.healthBar.transform.parent);
        }

        private void AddSharedIfSafe(Transform container)
        {
            if (!container || container == hud.transform)
                return;
            if (hud.mainContainer && container == hud.mainContainer.transform)
                return;
            Transform spring = HudBinder.FindSpringCanvas(hud);
            if (spring && (container == spring || container.parent == spring))
                return;
            if (container.GetComponentInChildren<ChatBox>(true))
                return;
            fader.AddShared(container);
        }

        private void Stop()
        {
            running = false;
            fader.Clear();
            foreach (HudWidget widget in widgets)
                widget.Unbind();
            bindTimer = 0f;
        }

        private bool IsScoreboardOpen()
        {
            return hud.scoreboardPanel && hud.scoreboardPanel.activeInHierarchy;
        }

        private static bool IsPeekHeld()
        {
            KeyboardShortcut shortcut = PluginConfig.PeekKey.Value;
            if (shortcut.MainKey == KeyCode.None || !Input.GetKey(shortcut.MainKey))
                return false;
            foreach (KeyCode modifier in shortcut.Modifiers)
            {
                if (!Input.GetKey(modifier))
                    return false;
            }
            return true;
        }
    }
}
