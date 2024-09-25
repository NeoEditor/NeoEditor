using System;
using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using UnityEngine;

namespace NeoEditor.Tabs
{
    public class TabBase : MonoBehaviour
    {
        public virtual void InitTab(Dictionary<string, LevelEventInfo> levelEventsInfo) { }

        public virtual void OnOpenLevel() { }

        public virtual void OnPlayLevel() { }

        public virtual void OnRemakePath() { }

        public virtual void OnLevelChanged() { }

        public virtual void OnActive() { }

        public virtual void OnInactive() { }

        public virtual void SelectEvent(LevelEvent levelEvent) { }

		public virtual void AddEvent(LevelEventType type) { }

        public virtual InspectorPanel GetLevelEventsPanel() { return null; }
	}
}
