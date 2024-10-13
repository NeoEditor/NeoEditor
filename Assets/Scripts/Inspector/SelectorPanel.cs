using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class SelectorPanel : MonoBehaviour
    {
        public RectTransform content;
        public RectTransform buttons;
        public TMP_Text title;

        List<EventInspectorPanel> categories = new List<EventInspectorPanel>();

        List<Button> tabButtons = new List<Button>();

        public void Init(LevelEventCategory[] allEvents)
        {
            NeoEditor editor = NeoEditor.Instance;

            foreach(var panel in categories)
            {
                if (panel != null) Destroy(panel.gameObject);
            }

			foreach (var button in tabButtons)
			{
				if (button != null) Destroy(button.gameObject);
			}

			categories = new List<EventInspectorPanel>();

            int i = 0;
            foreach(var eventCategory in allEvents)
            {
                var category = Instantiate(editor.prefab_eventInspector, content)
                    .GetComponent<EventInspectorPanel>();
                //category.parentTab = parentTab;
                category.Init(NeoConstants.Events[eventCategory].ToList());
                categories.Add(category);

                var button = Instantiate(editor.prefab_inspectorTabButton, buttons)
                    .GetComponent<Button>();
                int tab = i;
                button.onClick.AddListener(() => SelectTab(tab));
                tabButtons.Add(button);
                button.transform.GetChild(0).GetComponent<Image>().sprite = NeoConstants.CategoryIcons[eventCategory];
                i++;
            }

            SelectTab(0);
        }

        public void SelectTab(int index)
        {
            for (int i = 0; i < categories.Count; i++)
            {
				categories[i].gameObject.SetActive(i == index);
                tabButtons[i].interactable = i != index;
            }
        }
    }
}
