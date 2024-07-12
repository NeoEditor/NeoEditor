using System;
using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using NeoEditor.Tabs;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class FilterEventPanel : EventPanel
    {
        public FilterInspectorPanel controllerPanel;
        public FilterInspectorPanel sliderPanel;
        public Button switchToAdvanced;

		public override void Init(List<LevelEventInfo> infos)
		{
			base.Init(infos);
			switchToAdvanced.onClick.AddListener(() => { SwitchToAdvanced(selectedEvent); });
		}

		public void SwitchToAdvanced(LevelEvent filterEvent)
		{
			if (filterEvent == null || filterEvent.eventType != LevelEventType.SetFilter)
				return;
		}

		public override void SetProperties(LevelEventType type, LevelEvent levelEvent)
		{
			base.SetProperties(type, levelEvent);
			switchToAdvanced.gameObject.SetActive(type == LevelEventType.SetFilter);
		}

		public override void HidePanel()
		{
			base.HidePanel();
			switchToAdvanced.gameObject.SetActive(false);
		}
	}
}
