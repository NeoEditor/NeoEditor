using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using NeoEditor.Tabs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class DecorationPanel : InspectorPanel
    {
        public RectTransform content;

        public DecorationTab parentTab;

        PropertiesPanel decorations;
        LevelEvent decorationsEvent;

        public void Init(LevelEventInfo decoration)
        {
            NeoEditor editor = NeoEditor.Instance;

            if (decorations != null)
                Destroy(decorations.gameObject);
            decorations = Instantiate(editor.prefab_inspector, content)
                .GetComponent<PropertiesPanel>();
            decorationsEvent = editor.levelData.decorationSettings;

            panelsList = new List<PropertiesPanel> { decorations };

            decorations.Init(this, decoration);

			editor.propertyControlDecorationsList.OnItemSelected = editor.OnDecorationSelected;
			editor.propertyControlDecorationsList.OnAllItemsDeselected = editor.OnDecorationAllItemsDeselected;
		}

        public void SetProperties(LevelEvent decoration)
        {
            decorations.SetProperties(decoration, false);
        }
    }
}
