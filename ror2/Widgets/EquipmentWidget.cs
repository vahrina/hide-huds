using System;
using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HideHud.Widgets
{
    internal sealed class EquipmentWidget : PolledWidget
    {
        public EquipmentWidget(HudContext ctx) : base(ctx, PluginConfig.Equipment) { }

        protected override void ResolveRoots(List<Transform> roots)
        {
            if (Hud.equipmentIcons == null)
                return;
            foreach (EquipmentIcon icon in Hud.equipmentIcons)
            {
                if (icon)
                    roots.Add(icon.transform);
            }
        }

        protected override bool TrySample(out int hash)
        {
            Inventory inventory = Ctx.Inventory;
            if (!inventory)
            {
                hash = 0;
                return false;
            }
            EquipmentState current = inventory.currentEquipmentState;
            EquipmentState alternate = inventory.alternateEquipmentState;
            hash = HashCode.Combine(
                current.equipmentIndex, current.charges,
                alternate.equipmentIndex, alternate.charges,
                inventory.activeEquipmentSlot);
            return true;
        }

        protected override bool CanHide()
        {
            if (!PluginConfig.KeepEquipmentWhileRecharging.Value)
                return true;
            Inventory inventory = Ctx.Inventory;
            if (!inventory)
                return true;
            EquipmentState current = inventory.currentEquipmentState;
            return current.equipmentIndex == EquipmentIndex.None || !current.isPerfomingRecharge;
        }
    }
}
