using BepInEx.Configuration;
using UnityEngine;

namespace HideHud
{
    internal static class PluginConfig
    {
        public static ConfigEntry<bool> Enabled;
        public static ConfigEntry<float> IdleSeconds;
        public static ConfigEntry<float> FadeInSeconds;
        public static ConfigEntry<float> FadeOutSeconds;
        public static ConfigEntry<KeyboardShortcut> PeekKey;
        public static ConfigEntry<bool> ShowAllOnScoreboard;

        public static ConfigEntry<bool> KeepBossBarWhileActive;
        public static ConfigEntry<bool> KeepDifficultyTimer;
        public static ConfigEntry<bool> KeepEquipmentWhileRecharging;

        public static ConfigEntry<bool> Health;
        public static ConfigEntry<bool> Money;
        public static ConfigEntry<bool> Items;
        public static ConfigEntry<bool> Equipment;
        public static ConfigEntry<bool> Experience;
        public static ConfigEntry<bool> Buffs;
        public static ConfigEntry<bool> Skills;
        public static ConfigEntry<bool> Objectives;
        public static ConfigEntry<bool> Allies;
        public static ConfigEntry<bool> BossBar;
        public static ConfigEntry<bool> Difficulty;
        public static ConfigEntry<bool> Version;

        public static void Bind(ConfigFile cfg)
        {
            const string general = "General";
            Enabled = cfg.Bind(general, "Enabled", true,
                "turn the mod on or off, off shows the whole hud");
            IdleSeconds = cfg.Bind(general, "IdleSeconds", 3f,
                new ConfigDescription("seconds of nothing happening before stuff fades out",
                    new AcceptableValueRange<float>(0.5f, 60f)));
            FadeInSeconds = cfg.Bind(general, "FadeInSeconds", 0.15f,
                new ConfigDescription("fade in time, 0 is instant", new AcceptableValueRange<float>(0f, 5f)));
            FadeOutSeconds = cfg.Bind(general, "FadeOutSeconds", 0.35f,
                new ConfigDescription("fade out time, 0 is instant", new AcceptableValueRange<float>(0f, 5f)));
            PeekKey = cfg.Bind(general, "PeekKey", new KeyboardShortcut(KeyCode.LeftAlt),
                "hold to show everything, None to disable");
            ShowAllOnScoreboard = cfg.Bind(general, "ShowAllOnScoreboard", true,
                "show everything while the scoreboard is open");

            const string behavior = "Behavior";
            KeepBossBarWhileActive = cfg.Bind(behavior, "KeepBossBarWhileActive", false,
                "keep the boss bar up while a boss is alive");
            KeepDifficultyTimer = cfg.Bind(behavior, "KeepDifficultyTimer", false,
                "keep the run timer visible, rest of the difficulty panel still hides");
            KeepEquipmentWhileRecharging = cfg.Bind(behavior, "KeepEquipmentWhileRecharging", true,
                "keep equipment up while recharging");

            const string widgets = "Widgets";
            Health = cfg.Bind(widgets, "Health", true, "hide the health bar, stays up while hurt");
            Money = cfg.Bind(widgets, "Money", true, "hide gold, lunar and void coins");
            Items = cfg.Bind(widgets, "Items", true, "hide the item bar");
            Equipment = cfg.Bind(widgets, "Equipment", true, "hide equipment");
            Experience = cfg.Bind(widgets, "Experience", true, "hide level and xp bar");
            Buffs = cfg.Bind(widgets, "Buffs", true, "hide buffs");
            Skills = cfg.Bind(widgets, "Skills", true, "hide skills and the sprint / inventory hints");
            Objectives = cfg.Bind(widgets, "Objectives", true, "hide objectives");
            Allies = cfg.Bind(widgets, "Allies", true, "hide ally and drone cards, stays up while one is hurt");
            BossBar = cfg.Bind(widgets, "BossBar", false, "hide the boss bar");
            Difficulty = cfg.Bind(widgets, "Difficulty", true, "hide timer, difficulty, stage and artifacts");
            Version = cfg.Bind(widgets, "Version", true, "hide the version number in the top right");
        }
    }
}
