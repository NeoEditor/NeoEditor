using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class ProjectPanel : MonoBehaviour
    {
        public RectTransform content;
        public TextMeshProUGUI title;

        InspectorPanel[] inspectors;
        InspectorPanel songSettings;
        InspectorPanel levelSettings;
        InspectorPanel miscSettings;

        Button[] tabButtons;
        public Button songButton;
        public Button levelButton;
        public Button miscButton;

        public void Init(
            ADOFAI.LevelEventInfo song,
            ADOFAI.LevelEventInfo level,
            ADOFAI.LevelEventInfo misc
        )
        {
            NeoEditor editor = NeoEditor.Instance;
            songSettings = Instantiate(editor.prefab_inspector, content)
                .GetComponent<InspectorPanel>();
            songSettings.selectedEvent = editor.events.Find(e =>
                e.eventType == ADOFAI.LevelEventType.SongSettings
            );

            levelSettings = Instantiate(editor.prefab_inspector, content)
                .GetComponent<InspectorPanel>();
            levelSettings.selectedEvent = editor.events.Find(e =>
                e.eventType == ADOFAI.LevelEventType.LevelSettings
            );

            miscSettings = Instantiate(editor.prefab_inspector, content)
                .GetComponent<InspectorPanel>();
            miscSettings.selectedEvent = editor.events.Find(e =>
                e.eventType == ADOFAI.LevelEventType.MiscSettings
            );

            inspectors = new InspectorPanel[] { songSettings, levelSettings, miscSettings };
            tabButtons = new Button[] { songButton, levelButton, miscButton };

            for (int i = 0; i < inspectors.Length; i++)
            {
                int tab = i;
                tabButtons[i].onClick.AddListener(() => SelectTab(tab));
            }

            songSettings.Init(song);
            levelSettings.Init(level);
            miscSettings.Init(misc);

            SelectTab(0);
        }

        public void SelectTab(int index)
        {
            for (int i = 0; i < inspectors.Length; i++)
            {
                inspectors[i].gameObject.SetActive(i == index);
                tabButtons[i].interactable = i != index;
            }
        }
    }
}
