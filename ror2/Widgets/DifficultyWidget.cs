using System;
using System.Collections.Generic;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace HideHud.Widgets
{
    // timer, difficulty, stage and artifacts. objectives live in the same panel but have their own widget
    // timer ticks dont count or it would never hide
    internal sealed class DifficultyWidget : PolledWidget
    {
        private readonly List<Transform> excluded = new List<Transform>();
        private bool boundWithKeepTimer;

        public DifficultyWidget(HudContext ctx) : base(ctx, PluginConfig.Difficulty) { }

        public override bool BindingStale => boundWithKeepTimer != PluginConfig.KeepDifficultyTimer.Value;

        protected override void ResolveRoots(List<Transform> roots)
        {
            GameObject gameModeUi = Hud.gameModeUiInstance;
            if (!gameModeUi)
                return;
            Transform root = gameModeUi.transform;

            excluded.Clear();
            ObjectivePanelController objectives = ObjectivesWidget.FindPanel(Hud);
            if (objectives && objectives.transform.IsChildOf(root))
                excluded.Add(objectives.transform);

            boundWithKeepTimer = PluginConfig.KeepDifficultyTimer.Value;
            if (boundWithKeepTimer)
            {
                RunTimerUIController timer = gameModeUi.GetComponentInChildren<RunTimerUIController>(true);
                if (timer)
                    excluded.Add(HudBinder.ChildUnder(timer.transform, root));
            }

            HudBinder.CollectExcluding(root, excluded, roots);
        }

        protected override bool TrySample(out int hash)
        {
            Run run = Run.instance;
            if (!run)
            {
                hash = 0;
                return false;
            }
            hash = HashCode.Combine(run.stageClearCount, run.selectedDifficulty, run.ambientLevelFloor);
            return true;
        }
    }
}
