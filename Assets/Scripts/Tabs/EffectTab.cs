using ADOFAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Tabs
{
    public class EffectTab : TabBase
    {
		public RawImage gameView;
		public RawImage sceneView;

		public override void InitTab(Dictionary<string, LevelEventInfo> levelEventsInfo)
		{
			gameView.texture = Assets.GameRenderer;
			sceneView.texture = Assets.SceneRenderer;
		}
	}
}
