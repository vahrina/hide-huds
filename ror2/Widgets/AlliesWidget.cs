using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HideHud.Widgets
{
    // ally and drone cards, stays up while any ally is under 99% like hunkhud
    internal sealed class AlliesWidget : PolledWidget
    {
        private const float FullThreshold = 0.99f;

        private readonly List<AllyCardController> cards = new List<AllyCardController>();
        private bool anyInjured;

        public AlliesWidget(HudContext ctx) : base(ctx, PluginConfig.Allies) { }

        protected override void ResolveRoots(List<Transform> roots)
        {
            if (Hud.allyCardManager)
                roots.Add(Hud.allyCardManager.transform);
        }

        protected override bool TrySample(out int hash)
        {
            hash = 17;
            anyInjured = false;
            if (!Hud.allyCardManager)
                return false;

            Hud.allyCardManager.GetComponentsInChildren(false, cards);
            foreach (AllyCardController card in cards)
            {
                HealthComponent hc = GetHealth(card);
                float fraction = 1f;
                if (hc)
                {
                    float max = hc.fullHealth + hc.fullShield;
                    fraction = max > 0f ? (hc.health + hc.shield) / max : 1f;
                    if (hc.alive && fraction < FullThreshold)
                        anyInjured = true;
                }
                hash = hash * 31 + (card.sourceMaster ? card.sourceMaster.GetInstanceID() : 0);
                hash = hash * 31 + Mathf.RoundToInt(fraction * 200f);
            }
            hash = hash * 31 + cards.Count;
            cards.Clear();
            return true;
        }

        protected override bool CanHide() => !anyInjured;

        private static HealthComponent GetHealth(AllyCardController card)
        {
            if (!card)
                return null;
            if (card.healthBar && card.healthBar.source)
                return card.healthBar.source;
            CharacterMaster master = card.sourceMaster;
            CharacterBody body = master ? master.GetBody() : null;
            return body ? body.healthComponent : null;
        }
    }
}
