using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HideHud
{
    // whatever the hud is looking at, reads from the hud target so spectating and mp clients work
    internal sealed class HudContext
    {
        public readonly HUD Hud;
        public CharacterMaster Master { get; private set; }
        public CharacterBody Body { get; private set; }
        public HealthComponent Health { get; private set; }

        private GameObject bodyObject;

        public HudContext(HUD hud)
        {
            Hud = hud;
        }

        public Inventory Inventory => Master ? Master.inventory : null;

        public NetworkUser ViewerNetworkUser
        {
            get
            {
                LocalUser viewer = Hud.localUserViewer;
                return viewer != null ? viewer.currentNetworkUser : null;
            }
        }

        // true if master or body changed since last call
        public bool Refresh()
        {
            CharacterMaster master = Hud.targetMaster;
            GameObject body = Hud.targetBodyObject;
            if (master == Master && body == bodyObject)
                return false;

            Master = master;
            bodyObject = body;
            Body = body ? body.GetComponent<CharacterBody>() : null;
            Health = Body ? Body.healthComponent : null;
            return true;
        }
    }
}
