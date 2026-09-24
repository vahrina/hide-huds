using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace HideHud
{
    public class HideHud : Mod
    {
    }

    public enum HudPolicy
    {
        Idle,
        Map,
        Resource,
        Hotbar,
        Flight,
        Stealth
    }

    public class HideHudSystem : ModSystem
    {
        // keep in sync with ui_list.md
        private static readonly Dictionary<string, HudPolicy> Policies = new()
        {
            ["Vanilla: Map / Minimap"] = HudPolicy.Map,
            ["Vanilla: Resource Bars"] = HudPolicy.Resource,
            ["Vanilla: Info Accessories Bar"] = HudPolicy.Idle,
            ["Vanilla: Hotbar"] = HudPolicy.Hotbar,
            ["Vanilla: Builder Accessories Bar"] = HudPolicy.Idle,
            ["Vanilla: Radial Hotbars"] = HudPolicy.Idle,
            ["Flight UI"] = HudPolicy.Flight,
            ["Stealth UI"] = HudPolicy.Stealth,
            ["Rage and Adrenaline UI"] = HudPolicy.Idle,
        };

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (layers == null)
                return;

            foreach (var layer in layers)
            {
                if (layer?.Name == null || !Policies.TryGetValue(layer.Name, out var policy))
                    continue;

                bool hud = HideHudPlayer.HudUp;
                bool show = policy switch
                {
                    HudPolicy.Idle => hud,
                    HudPolicy.Map => hud || Main.mapStyle == 1,
                    HudPolicy.Resource => HideHudPlayer.ResourceAlpha > 0.01f,
                    HudPolicy.Hotbar => hud || HideHudPlayer.HotbarShowTimer > 0,
                    HudPolicy.Flight => hud || FlightNotFull(),
                    HudPolicy.Stealth => StealthNotFull(),
                    _ => true
                };

                layer.Active = show;
            }
        }

        private static bool FlightNotFull()
        {
            Player p = Main.LocalPlayer;
            if (p is not { active: true } || p.dead)
                return false;

            return p.wingTimeMax > 0 && p.wingTime < p.wingTimeMax
                || (p.wingsLogic > 0 || p.rocketBoots > 0) && p.controlJump && p.wingTime > 0;
        }

        private static bool StealthNotFull()
        {
            Player p = Main.LocalPlayer;
            if (p is not { active: true } || p.dead)
                return false;

            return CalamityStealth.NotFull(p);
        }
    }

    internal static class CalamityStealth
    {
        private static bool _looked;
        private static FieldInfo _cur;
        private static FieldInfo _max;

        public static bool NotFull(Player p)
        {
            if (!_looked)
            {
                _looked = true;
                foreach (var mp in p.ModPlayers)
                {
                    if (mp?.Name != "CalamityPlayer")
                        continue;
                    var t = mp.GetType();
                    _cur = t.GetField("rogueStealth");
                    _max = t.GetField("rogueStealthMax");
                    break;
                }
            }

            if (_cur == null || _max == null)
                return false;

            try
            {
                foreach (var mp in p.ModPlayers)
                {
                    if (mp?.Name != "CalamityPlayer")
                        continue;
                    float cur = Convert.ToSingle(_cur.GetValue(mp));
                    float max = Convert.ToSingle(_max.GetValue(mp));
                    return max > 0f && cur < max;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }

    public class HideHudPlayer : ModPlayer
    {
        public static bool HudUp = true;
        public static float ResourceAlpha = 1f;
        public static int HotbarShowTimer;

        private int idleTimer;
        private int damageShowTimer;
        private int lastLife = -1;
        private int lastSelectedItem = -1;

        private const int HideDelay = 5 * 60;
        private const int HotbarShowDelay = 3 * 60;
        private const float FadeSpeed = 1f / 20f;

        public override void OnEnterWorld()
        {
            HudUp = true;
            idleTimer = 0;
            ResourceAlpha = 1f;
            Main.NewText($"{Mod.DisplayName}: initialized");
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            if (Player.whoAmI != Main.myPlayer || triggersSet == null)
                return;

            if (HotbarActivity(triggersSet))
                HotbarShowTimer = HotbarShowDelay;
            else if (HotbarShowTimer > 0)
                HotbarShowTimer--;

            bool busy = Main.playerInventory || Main.mapFullscreen || PlayerInput.ScrollWheelDelta != 0;
            if (busy)
            {
                idleTimer = 0;
                HudUp = true;
            }
            else if (idleTimer < HideDelay && ++idleTimer >= HideDelay)
            {
                HudUp = false;
            }
        }

        private bool HotbarActivity(TriggersSet t)
        {
            bool changed = Player.selectedItem != lastSelectedItem;
            lastSelectedItem = Player.selectedItem;

            bool key = t.Hotbar1 || t.Hotbar2 || t.Hotbar3 || t.Hotbar4 || t.Hotbar5
                || t.Hotbar6 || t.Hotbar7 || t.Hotbar8 || t.Hotbar9 || t.Hotbar10
                || t.HotbarMinus || t.HotbarPlus;

            return changed || key || PlayerInput.ScrollWheelDelta != 0;
        }

        public override void PostUpdate()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            if (lastLife >= 0 && Player.statLife < lastLife)
                damageShowTimer = HideDelay;
            lastLife = Player.statLife;

            if (damageShowTimer > 0)
                damageShowTimer--;

            bool hpNotFull = Player.statLifeMax2 > 0 && Player.statLife < Player.statLifeMax2;
            bool want = HudUp || hpNotFull || damageShowTimer > 0;
            ResourceAlpha = MathHelper.Clamp(ResourceAlpha + (want ? FadeSpeed : -FadeSpeed), 0f, 1f);
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            damageShowTimer = HideDelay;
            if (ResourceAlpha < 0.05f)
                ResourceAlpha = 0.05f;
        }
    }

    public class ResourceFadeOverlay : ModResourceOverlay
    {
        public override bool PreDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, ref Color textColor, out bool drawText)
        {
            float a = HideHudPlayer.ResourceAlpha;
            if (a <= 0.01f)
            {
                drawText = false;
                return false;
            }

            textColor *= a;
            drawText = true;
            return true;
        }

        public override bool PreDrawResource(ResourceOverlayDrawContext context)
        {
            float a = HideHudPlayer.ResourceAlpha;
            if (a <= 0.01f)
                return false;
            if (a >= 0.99f || context.texture == null)
                return true;

            context.color *= a;
            context.Draw();
            return false;
        }
    }
}
