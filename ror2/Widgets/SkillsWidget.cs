using System.Collections.Generic;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;

namespace HideHud.Widgets
{
    // skills plus the sprint and inventory hints, stays up until every skill is off cooldown
    internal sealed class SkillsWidget : HudWidget
    {
        private const int SlotCount = 4;

        private readonly GenericSkill[] skills = new GenericSkill[SlotCount];
        private readonly SkillDef[] lastDef = new SkillDef[SlotCount];
        private readonly int[] lastStock = new int[SlotCount];
        private readonly float[] lastCooldown = new float[SlotCount];
        private bool hasSample;

        public SkillsWidget(HudContext ctx) : base(ctx, PluginConfig.Skills) { }

        protected override void ResolveRoots(List<Transform> roots)
        {
            if (Hud.skillIcons == null)
                return;

            Transform scaler = null;
            foreach (SkillIcon icon in Hud.skillIcons)
            {
                if (icon)
                {
                    scaler = icon.transform.parent;
                    break;
                }
            }

            if (scaler && scaler.parent == HudBinder.FindCluster(Hud, "BottomRightCluster"))
            {
                // everything under the scaler except equipment, that has its own widget
                foreach (Transform child in scaler)
                {
                    if (!child.GetComponentInChildren<EquipmentIcon>(true))
                        roots.Add(child);
                }
                return;
            }

            foreach (SkillIcon icon in Hud.skillIcons)
            {
                if (icon)
                    roots.Add(icon.transform);
            }
            if (Hud.sprintIcon)
                roots.Add(Hud.sprintIcon.transform);
        }

        public override void OnTargetChanged()
        {
            hasSample = false;
        }

        protected override bool CheckForActivity()
        {
            CharacterBody body = Ctx.Body;
            SkillLocator locator = body ? body.skillLocator : null;
            if (!locator)
            {
                hasSample = false;
                return false;
            }

            skills[0] = locator.primary;
            skills[1] = locator.secondary;
            skills[2] = locator.utility;
            skills[3] = locator.special;

            bool changed = false;
            for (int i = 0; i < SlotCount; i++)
            {
                GenericSkill skill = skills[i];
                SkillDef def = skill ? skill.skillDef : null;
                int stock = skill ? skill.stock : 0;
                float cooldown = skill ? skill.cooldownRemaining : 0f;

                if (hasSample)
                {
                    // cooldown went up so the skill was just used
                    if (def != lastDef[i] || stock != lastStock[i] || cooldown > lastCooldown[i] + 0.1f)
                        changed = true;
                }
                lastDef[i] = def;
                lastStock[i] = stock;
                lastCooldown[i] = cooldown;
            }
            hasSample = true;
            return changed;
        }

        protected override bool CanHide()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                GenericSkill skill = skills[i];
                if (!skill || !skill.skillDef)
                    continue;
                // skip skills that never recharge or theyd keep it up forever
                if (skill.maxStock <= 0 || skill.rechargeStock <= 0 || skill.isCooldownBlocked)
                    continue;
                if (skill.stock < skill.maxStock)
                    return false;
            }
            return true;
        }
    }
}
