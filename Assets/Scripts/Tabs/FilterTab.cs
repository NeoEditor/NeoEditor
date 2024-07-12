using ADOFAI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeoEditor.Tabs
{
	public class FilterTab : EffectTabBase
	{
		public override void InitTab(Dictionary<string, LevelEventInfo> levelEventsInfo)
		{
			List<LevelEventInfo> infos = new List<LevelEventInfo>()
			{
				GCS.levelEventsInfo["SetFilter"],
				GCS.levelEventsInfo["SetFilterAdvanced"],
				GCS.levelEventsInfo["HallOfMirrors"],
				GCS.levelEventsInfo["ShakeScreen"],
				GCS.levelEventsInfo["Bloom"],
				GCS.levelEventsInfo["ScreenTile"],
				GCS.levelEventsInfo["ScreenScroll"],
			};

			if (GCS.levelEventsInfo.ContainsKey("SetFrameRate"))
				infos.Add(GCS.levelEventsInfo["SetFrameRate"]);

			events.Init(infos);

			if (gameView != null) gameView.texture = Assets.GameRenderer;
		}
	}
}
