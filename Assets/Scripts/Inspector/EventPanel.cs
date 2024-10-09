using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class EventPanel : InspectorPanel
    {
        public RectTransform content;
        public SelectorPanel selectorPanel;
        public LevelEventCategory[] categories;

        public Dictionary<LevelEventType, PropertiesPanel> inspectors;

        public virtual void Init(List<LevelEventInfo> infos)
        {
            NeoEditor editor = NeoEditor.Instance;

            inspectors = new Dictionary<LevelEventType, PropertiesPanel>();
			panelsList = new List<PropertiesPanel>();

			foreach (var info in infos)
            {
                PropertiesPanel panel = Instantiate(editor.prefab_inspector, content)
                    .GetComponent<PropertiesPanel>();
                //panel.parentTab = parentTab;
                panel.Init(this, info);
                panel.gameObject.SetActive(false);
                inspectors.Add(info.type, panel);
				if (info.type == LevelEventType.EditorComment)
				{
					panelsList.Insert(0, panel);
				}
				else
				{
					panelsList.Add(panel);
				}
			}

            selectorPanel.Init(categories);
            selectorPanel.title = title;
		}

        public virtual void SetProperties(LevelEventType type, LevelEvent levelEvent)
        {
            selectorPanel.gameObject.SetActive(false);
			title.text =
                type == LevelEventType.None
                    ? ""
                    : RDString.Get("editor." + type.ToString(), null, LangSection.Translations);
            selectedEvent = levelEvent;
            selectedEventType = type;
            inspectors[type].gameObject.SetActive(true);
            inspectors[type].SetProperties(levelEvent);
        }

        public virtual void SetSelector()
        {
            title.text = "Select Event";
            selectedEventType = LevelEventType.None;
            selectorPanel.gameObject.SetActive(true);
            selectedEvent = null;
        }

        public virtual void HidePanel()
        {
            title.text = "";
			if (selectedEventType == LevelEventType.None)
                selectorPanel.gameObject.SetActive(false);
			else
				inspectors[selectedEventType].gameObject.SetActive(false);
		}
	}
}
