using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using NeoEditor.Tabs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class ProjectPanel : InspectorPanel
    {
        public RectTransform content;

        public ProjectTab parentTab;

        PropertiesPanel songSettings;
        PropertiesPanel levelSettings;
        PropertiesPanel miscSettings;
        LevelEvent[] settingEvents = new LevelEvent[3];

        Button[] tabButtons;
        public Button songButton;
        public Button levelButton;
        public Button miscButton;

        public void Init(LevelEventInfo song, LevelEventInfo level, LevelEventInfo misc)
        {
            NeoEditor editor = NeoEditor.Instance;

            if (songSettings != null)
                Destroy(songSettings.gameObject);
            songSettings = Instantiate(editor.prefab_inspector, content)
                .GetComponent<PropertiesPanel>();
            settingEvents[0] = editor.levelData.songSettings;

			if (levelSettings != null)
                Destroy(levelSettings.gameObject);
            levelSettings = Instantiate(editor.prefab_inspector, content)
                .GetComponent<PropertiesPanel>();
			settingEvents[1] = editor.levelData.levelSettings;

            if (miscSettings != null)
                Destroy(miscSettings.gameObject);
            miscSettings = Instantiate(editor.prefab_inspector, content)
                .GetComponent<PropertiesPanel>();
            settingEvents[2] = editor.levelData.miscSettings;

            panelsList = new List<PropertiesPanel> { songSettings, levelSettings, miscSettings };
            tabButtons = new Button[] { songButton, levelButton, miscButton };

            for (int i = 0; i < panelsList.Count; i++)
            {
                int tab = i;
                tabButtons[i].onClick.AddListener(() => SelectTab(tab));
            }

            songSettings.Init(this, song);
            levelSettings.Init(this, level);
            miscSettings.Init(this, misc);

            //songSettings.parentTab = parentTab;
            //levelSettings.parentTab = parentTab;
            //miscSettings.parentTab = parentTab;
        }

        public void SetProperties(LevelEvent song, LevelEvent level, LevelEvent misc)
        {
            songSettings.SetProperties(song, false);
            levelSettings.SetProperties(level, false);
            miscSettings.SetProperties(misc, false);
        }

        public void SelectTab(int index)
        {
            selectedEvent = settingEvents[index];
            selectedEventType = settingEvents[index].eventType;
            for (int i = 0; i < panelsList.Count; i++)
            {
                panelsList[i].gameObject.SetActive(i == index);
                tabButtons[i].interactable = i != index;
            }
        }
    }
}
