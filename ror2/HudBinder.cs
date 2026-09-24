using System.Collections.Generic;
using RoR2.UI;
using UnityEngine;

namespace HideHud
{
    internal static class HudBinder
    {
        private static bool hooked;

        public static void Enable()
        {
            if (!hooked)
            {
                On.RoR2.UI.HUD.Awake += HUD_Awake;
                hooked = true;
            }
            foreach (HUD hud in HUD.readOnlyInstanceList)
                Attach(hud);
        }

        public static void Disable()
        {
            if (hooked)
            {
                On.RoR2.UI.HUD.Awake -= HUD_Awake;
                hooked = false;
            }
            foreach (HUD hud in HUD.readOnlyInstanceList)
            {
                if (hud && hud.TryGetComponent(out HudAutoHideController controller))
                    Object.Destroy(controller);
            }
        }

        private static void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
            orig(self);
            Attach(self);
        }

        private static void Attach(HUD hud)
        {
            if (!hud || hud.GetComponent<HudAutoHideController>())
                return;
            hud.gameObject.AddComponent<HudAutoHideController>();
        }

        public static Transform FindSpringCanvas(HUD hud)
        {
            if (hud.TryGetComponent(out ChildLocator locator) && locator.TryFindChild("SpringCanvas", out Transform spring) && spring)
                return spring;
            return hud.mainContainer ? hud.mainContainer.transform.Find("MainUIArea/SpringCanvas") : null;
        }

        // ChildLocator only has some clusters, fall back to SpringCanvas children
        public static Transform FindCluster(HUD hud, string name)
        {
            if (hud.TryGetComponent(out ChildLocator locator) && locator.TryFindChild(name, out Transform found) && found)
                return found;
            Transform spring = FindSpringCanvas(hud);
            return spring ? spring.Find(name) : null;
        }

        // ancestor of t right under container, or t if container isnt above it
        public static Transform ChildUnder(Transform t, Transform container)
        {
            if (!t || !container)
                return t;
            for (Transform cur = t; cur; cur = cur.parent)
            {
                if (cur.parent == container)
                    return cur;
            }
            return t;
        }

        // children of root, only digs into branches that hold an excluded transform
        public static void CollectExcluding(Transform root, IList<Transform> excluded, List<Transform> result)
        {
            if (!root)
                return;
            foreach (Transform child in root)
            {
                bool isExcluded = false;
                bool containsExcluded = false;
                foreach (Transform ex in excluded)
                {
                    if (!ex)
                        continue;
                    if (ex == child)
                        isExcluded = true;
                    else if (ex.IsChildOf(child))
                        containsExcluded = true;
                }
                if (isExcluded)
                    continue;
                if (containsExcluded)
                    CollectExcluding(child, excluded, result);
                else
                    result.Add(child);
            }
        }
    }
}
