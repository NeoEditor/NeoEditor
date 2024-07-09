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
        public EffectTabBase parentTab;

        protected Dictionary<LevelEventType, InspectorPanel> inspectors;
        protected EventInspectorPanel eventSelector;
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
                inspector.Init(info, true);
                inspector.parentTab = parentTab;
                inspector.gameObject.SetActive(false);
                inspectors.Add(info.type, inspector);
            }
			EventInspectorPanel select = Instantiate(editor.prefab_eventInspector, content)
				.GetComponent<EventInspectorPanel>();
            select.Init(infos.Select((i) => i.type).ToList());
            eventSelector = select;
            select.gameObject.SetActive(false);

            SetSelector();
		}

        public virtual void SetProperties(LevelEventType type, LevelEvent levelEvent)
        {
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
            eventSelector.gameObject.SetActive(true);
            selectedEvent = null;
        }

        public void HidePanel()
        {
			if (selectedEventType == LevelEventType.None)
				eventSelector.gameObject.SetActive(false);
			else
				inspectors[selectedEventType].gameObject.SetActive(false);
		}
	}
}
