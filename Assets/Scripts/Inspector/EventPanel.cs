using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using NeoEditor.Tabs;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class EventPanel : MonoBehaviour
    {
        public RectTransform content;
        public TextMeshProUGUI title;
        public TextMeshProUGUI eventTitle;
        public SelectorPanel selectorPanel;
        public EffectTabBase parentTab;
        public LevelEventCategory[] categories;

        protected Dictionary<LevelEventType, InspectorPanel> inspectors;
        protected LevelEventType selectedEventType = LevelEventType.None;
        protected LevelEvent selectedEvent;

        public virtual void Init(List<LevelEventInfo> infos)
        {
            NeoEditor editor = NeoEditor.Instance;

            inspectors = new Dictionary<LevelEventType, InspectorPanel>();
            foreach (var info in infos)
            {
                InspectorPanel inspector = Instantiate(editor.prefab_inspector, content)
                    .GetComponent<InspectorPanel>();
                inspector.parentTab = parentTab;
                inspector.Init(info, true);
                inspector.gameObject.SetActive(false);
                inspectors.Add(info.type, inspector);
            }

            selectorPanel.parentTab = parentTab;
            selectorPanel.Init(categories);
            selectorPanel.title = eventTitle;
		}

        public virtual void SetProperties(LevelEventType type, LevelEvent levelEvent)
        {
            selectorPanel.gameObject.SetActive(false);
			eventTitle.text =
                type == LevelEventType.None
                    ? ""
                    : RDString.Get("editor." + type.ToString(), null, LangSection.Translations);
            selectedEventType = type;
            inspectors[type].gameObject.SetActive(true);
            inspectors[type].SetProperties(levelEvent);
            selectedEvent = levelEvent;
        }

        public virtual void SetSelector()
        {
            eventTitle.text = "Select Event";
            selectedEventType = LevelEventType.None;
            selectorPanel.gameObject.SetActive(true);
            selectedEvent = null;
        }

        public virtual void HidePanel()
        {
            eventTitle.text = "";
			if (selectedEventType == LevelEventType.None)
                selectorPanel.gameObject.SetActive(false);
			else
				inspectors[selectedEventType].gameObject.SetActive(false);
		}
	}
}
