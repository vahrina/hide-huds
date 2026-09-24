using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace HideHud.Widgets
{
    // item bar, onInventoryChanged also fires for equipment charges so hash stacks to catch real changes
    internal sealed class ItemsWidget : HudWidget
    {
        private Inventory subscribed;
        private bool eventPending;
        private int lastHash;

        public ItemsWidget(HudContext ctx) : base(ctx, PluginConfig.Items) { }

        protected override void ResolveRoots(List<Transform> roots)
        {
            if (!Hud.itemInventoryDisplay)
                return;
            Transform cluster = HudBinder.FindCluster(Hud, "TopCenterCluster");
            roots.Add(HudBinder.ChildUnder(Hud.itemInventoryDisplay.transform, cluster));
        }

        public override void OnTargetChanged()
        {
            Subscribe(Ctx.Inventory);
            lastHash = HashItems(subscribed);
            eventPending = false;
        }

        protected override void OnUnbind()
        {
            Subscribe(null);
        }

        protected override bool CheckForActivity()
        {
            if (subscribed != Ctx.Inventory)
                OnTargetChanged();
            if (!eventPending)
                return false;
            eventPending = false;
            int hash = HashItems(subscribed);
            if (hash == lastHash)
                return false;
            lastHash = hash;
            return true;
        }

        private void Subscribe(Inventory inventory)
        {
            if ((object)subscribed == inventory)
                return;
            if ((object)subscribed != null)
                subscribed.onInventoryChanged -= OnInventoryChanged;
            subscribed = inventory;
            if (inventory)
                inventory.onInventoryChanged += OnInventoryChanged;
        }

        private void OnInventoryChanged()
        {
            eventPending = true;
        }

        private static int HashItems(Inventory inventory)
        {
            if (!inventory)
                return 0;
            int hash = 17;
            List<ItemIndex> order = inventory.itemAcquisitionOrder;
            for (int i = 0; i < order.Count; i++)
            {
                ItemIndex index = order[i];
                hash = hash * 31 + (int)index;
                hash = hash * 31 + inventory.GetItemCountPermanent(index);
                hash = hash * 31 + inventory.GetItemCountTemp(index);
            }
            return hash;
        }
    }
}
