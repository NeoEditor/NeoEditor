using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using NeoEditor.Inspector.Timeline;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Tabs
{
    public class EffectTabBase : TabBase
    {
        public RawImage gameView;
        public RawImage sceneView;

        public RectTransform timelineParent;
        public TimelinePanel timeline;

        //public EventPanel inspector;

        protected bool needReset;

        public override void InitTab(Dictionary<string, LevelEventInfo> levelEventsInfo)
        {
            gameView.texture = Assets.GameRenderer;
            sceneView.texture = Assets.SceneRenderer;
        }

        public override void OnOpenLevel()
        {
            timeline.Init();
        }

        public override void OnRemakePath()
        {
            needReset = true;
        }

        public override void OnActive()
        {
            timeline.SetParent(timelineParent);
            timeline.tab = this;
            if (needReset)
            {
                timeline.Init();
                needReset = false;
            }
        }

        public override void SelectEvent(LevelEvent levelEvent)
        {
            //inspector.selectasdf
        }
    }
}
