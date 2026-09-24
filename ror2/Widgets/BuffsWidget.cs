using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace HideHud.Widgets
{
    // buff row, only buff type and stacks count so long buffs dont keep it up
    internal sealed class BuffsWidget : PolledWidget
    {
        private bool[] hidden;

        public BuffsWidget(HudContext ctx) : base(ctx, PluginConfig.Buffs) { }

        protected override void ResolveRoots(List<Transform> roots)
        {
            if (Hud.buffDisplay)
                roots.Add(Hud.buffDisplay.transform);
        }

        protected override bool TrySample(out int hash)
        {
            hash = 0;
            CharacterBody body = Ctx.Body;
            if (!body)
                return false;

            int count = BuffCatalog.buffCount;
            if (hidden == null || hidden.Length != count)
            {
                hidden = new bool[count];
                for (int i = 0; i < count; i++)
                {
                    BuffDef def = BuffCatalog.GetBuffDef((BuffIndex)i);
                    hidden[i] = !def || def.isHidden;
                }
            }

            hash = 17;
            for (int i = 0; i < count; i++)
            {
                if (hidden[i])
                    continue;
                int stacks = body.GetBuffCount((BuffIndex)i);
                if (stacks > 0)
                    hash = (hash * 31 + i) * 31 + stacks;
            }
            return true;
        }
    }
}
