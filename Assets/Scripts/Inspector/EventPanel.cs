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
    public class EventPanel : MonoBehaviour
    {
        public RectTransform content;
        public TextMeshProUGUI title;
        public TextMeshProUGUI eventTitle;
        public EffectTabBase parentTab;

        private Dictionary<LevelEventType, InspectorPanel> inspectors;
        private LevelEventType selectedEventType = LevelEventType.None;

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
        }

        public void HidePanel()
        {
            inspectors[selectedEventType].gameObject.SetActive(false);
        }
    }
}
