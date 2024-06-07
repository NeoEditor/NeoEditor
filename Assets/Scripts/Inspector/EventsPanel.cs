using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class EventsPanel : MonoBehaviour
    {
        public RectTransform content;
        public TextMeshProUGUI title;
        public TextMeshProUGUI eventTitle;

        private Dictionary<LevelEventType, InspectorPanel> inspectors;
        private LevelEventType selectedEventType = LevelEventType.None;

        public void Init(List<LevelEventInfo> infos)
        {
            NeoEditor editor = NeoEditor.Instance;

            inspectors = new Dictionary<LevelEventType, InspectorPanel>();
            foreach (var info in infos)
            {
                InspectorPanel inspector = Instantiate(editor.prefab_inspector, content)
                    .GetComponent<InspectorPanel>();
                inspector.Init(info);
                inspector.gameObject.SetActive(false);
                inspectors.Add(info.type, inspector);
            }
        }

        public void SetProperties(LevelEventType type, LevelEvent levelEvent)
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
