using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace HideHud.Widgets
{
    // gold, void and lunar coins top left
    internal sealed class MoneyWidget : PolledWidget
    {
        public MoneyWidget(HudContext ctx) : base(ctx, PluginConfig.Money) { }

        protected override void ResolveRoots(List<Transform> roots)
        {
            Transform cluster = HudBinder.FindCluster(Hud, "UpperLeftCluster");
            if (cluster && Hud.moneyText && Hud.moneyText.transform.IsChildOf(cluster))
            {
                roots.Add(cluster);
                return;
            }
            if (Hud.moneyText)
                roots.Add(Hud.moneyText.transform);
            if (Hud.lunarCoinContainer)
                roots.Add(Hud.lunarCoinContainer.transform);
            if (Hud.voidCoinContainer)
                roots.Add(Hud.voidCoinContainer.transform);
        }

        protected override bool TrySample(out int hash)
        {
            CharacterMaster master = Ctx.Master;
            NetworkUser user = Ctx.ViewerNetworkUser;
            uint money = master ? master.money : 0u;
            uint voidCoins = master ? master.voidCoins : 0u;
            uint lunarCoins = user ? user.lunarCoins : 0u;
            hash = HashCode.Combine(money, voidCoins, lunarCoins);
            return master || user;
        }
    }
}
