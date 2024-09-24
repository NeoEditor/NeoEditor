using ADOFAI;
using NeoEditor.Inspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoEditor.Tabs
{
    public class DecorationTab : EffectTabBase
    {
		public DecorationPanel decorations;

		public override void InitTab(Dictionary<string, LevelEventInfo> levelEventsInfo)
		{
			base.InitTab(levelEventsInfo);
			decorations.Init(levelEventsInfo["DecorationSettings"]);
			OnOpenLevel();
		}

		public override void OnOpenLevel()
		{
			NeoEditor editor = NeoEditor.Instance;
			decorations.SetProperties(editor.levelData.decorationSettings);
		}
	}
}
