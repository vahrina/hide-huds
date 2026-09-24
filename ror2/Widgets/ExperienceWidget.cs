using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace HideHud.Widgets
{
    // level and xp bar, buffs share the parent but hide separately
    internal sealed class ExperienceWidget : PolledWidget
    {
        public ExperienceWidget(HudContext ctx) : base(ctx, PluginConfig.Experience) { }

        protected override void ResolveRoots(List<Transform> roots)
        {
            if (Hud.levelText)
                roots.Add(Hud.levelText.transform);
            if (Hud.expBar && (!Hud.levelText || Hud.expBar.transform != Hud.levelText.transform))
                roots.Add(Hud.expBar.transform);
        }

        protected override bool TrySample(out int hash)
        {
            CharacterMaster master = Ctx.Master;
            CharacterBody body = Ctx.Body;
            if (!master && !body)
            {
                hash = 0;
                return false;
            }
            ulong experience = 0;
            if (master && TeamManager.instance)
                experience = TeamManager.instance.GetTeamExperience(master.teamIndex);
            int level = body ? Mathf.FloorToInt(body.level) : 0;
            hash = HashCode.Combine(experience, level);
            return true;
        }
    }
}
