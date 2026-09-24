using System.Collections.Generic;
using UnityEngine;

namespace HideHud
{
    // handles every CanvasGroup. nested groups multiply alpha so each one gets desired / parent desired
    // shared containers like BarRoots follow the most visible child
    internal sealed class HudFader
    {
        private sealed class Node
        {
            public Transform Transform;
            public CanvasGroup Group;
            public bool OriginalBlocksRaycasts;
            public bool OriginalInteractable;
            public HudWidget Widget;
            public Node Parent;
            public readonly List<Node> Children = new List<Node>();
            public float Desired;
        }

        private readonly List<Node> nodes = new List<Node>();
        private readonly Dictionary<Transform, Node> byTransform = new Dictionary<Transform, Node>();
        private bool dirty;

        public void AddWidget(HudWidget widget)
        {
            foreach (Transform root in widget.Roots)
            {
                if (!root || byTransform.ContainsKey(root))
                    continue;
                AddNode(root, widget);
            }
        }

        public void AddShared(Transform container)
        {
            if (!container || byTransform.ContainsKey(container))
                return;
            AddNode(container, null);
        }

        public void RemoveWidget(HudWidget widget)
        {
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                if (nodes[i].Widget != widget)
                    continue;
                Restore(nodes[i]);
                byTransform.Remove(nodes[i].Transform);
                nodes.RemoveAt(i);
                dirty = true;
            }
        }

        public bool HasLiveGroup(HudWidget widget)
        {
            foreach (Node node in nodes)
            {
                if (node.Widget == widget && node.Group)
                    return true;
            }
            return false;
        }

        public void Clear()
        {
            foreach (Node node in nodes)
                Restore(node);
            nodes.Clear();
            byTransform.Clear();
            dirty = false;
        }

        public void Apply()
        {
            if (dirty)
                Relink();

            foreach (Node node in nodes)
            {
                if (node.Parent == null)
                    ComputeDesired(node);
            }

            foreach (Node node in nodes)
            {
                if (!node.Group)
                    continue;
                float parentDesired = node.Parent != null ? node.Parent.Desired : 1f;
                float local = parentDesired <= 0.001f ? 0f : Mathf.Clamp01(node.Desired / parentDesired);
                if (Mathf.Abs(node.Group.alpha - local) > 0.0001f)
                    node.Group.alpha = local;

                bool shown = node.Desired > 0.001f;
                bool blocks = shown && node.OriginalBlocksRaycasts;
                bool interactable = shown && node.OriginalInteractable;
                if (node.Group.blocksRaycasts != blocks)
                    node.Group.blocksRaycasts = blocks;
                if (node.Group.interactable != interactable)
                    node.Group.interactable = interactable;
            }
        }

        private void AddNode(Transform t, HudWidget widget)
        {
            var node = new Node { Transform = t, Widget = widget };
            node.Group = t.GetComponent<CanvasGroup>();
            if (!node.Group)
                node.Group = t.gameObject.AddComponent<CanvasGroup>();
            node.OriginalBlocksRaycasts = node.Group.blocksRaycasts;
            node.OriginalInteractable = node.Group.interactable;
            nodes.Add(node);
            byTransform[t] = node;
            dirty = true;
        }

        private void Relink()
        {
            dirty = false;
            foreach (Node node in nodes)
            {
                node.Parent = null;
                node.Children.Clear();
            }
            foreach (Node node in nodes)
            {
                if (!node.Transform)
                    continue;
                for (Transform p = node.Transform.parent; p; p = p.parent)
                {
                    if (byTransform.TryGetValue(p, out Node parent))
                    {
                        node.Parent = parent;
                        parent.Children.Add(node);
                        break;
                    }
                }
            }
        }

        private static float ComputeDesired(Node node)
        {
            float childMax = -1f;
            foreach (Node child in node.Children)
                childMax = Mathf.Max(childMax, ComputeDesired(child));

            if (node.Widget != null)
                node.Desired = node.Widget.Alpha;
            else
                node.Desired = childMax < 0f ? 1f : childMax;
            return node.Desired;
        }

        // leave our groups at alpha 1 instead of destroying them so a quick rebind doesnt grab a dying one
        private static void Restore(Node node)
        {
            if (!node.Group)
                return;
            node.Group.alpha = 1f;
            node.Group.blocksRaycasts = node.OriginalBlocksRaycasts;
            node.Group.interactable = node.OriginalInteractable;
        }
    }
}
