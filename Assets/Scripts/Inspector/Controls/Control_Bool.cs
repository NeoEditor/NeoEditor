using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Controls
{
    public class Control_Bool : ControlBase
    {
        public Button onButton;
        public Button offButton;

        private bool _value;

        private readonly Color selectedColor = new Color(0.7529413f, 0.7529413f, 0.7529413f);
        private readonly Color unselectedColor = new Color(0.1882353f, 0.1882353f, 0.1882353f);

        public override List<Selectable> selectables =>
            new List<Selectable> { onButton, offButton };

        public bool value
        {
            get => _value;
            set
            {
                _value = value;
                UpdateButtons();
            }
        }

        public override void Setup(bool addListener)
        {
            if (addListener)
            {
                onButton.onClick.AddListener(
                    delegate()
                    {
                        SetValue(true);
                    }
                );
                offButton.onClick.AddListener(
                    delegate()
                    {
                        SetValue(false);
                    }
                );
            }
        }

        private void SetValue(bool on)
        {
            NeoEditor editor = NeoEditor.Instance;
            using (new SaveStateScope(editor, false, true, false))
            {
                LevelEvent selectedEvent = inspectorPanel.selectedEvent;
                value = on;
                selectedEvent[propertyInfo.name] = on;
                ToggleOthersEnabled();
                if (selectedEvent.eventType == LevelEventType.BackgroundSettings)
                {
                    ADOBase.customLevel.SetBackground();
                }
                else if (selectedEvent.IsDecoration)
                {
                    editor.UpdateDecorationObject(selectedEvent);
                }
                else if (selectedEvent.eventType == LevelEventType.SetFilterAdvanced)
                {
                    PropertyControl_FilterProperties propertyControl_FilterProperties =
                        inspectorPanel.properties["filterProperties"].control
                        as PropertyControl_FilterProperties;
                    propertyControl_FilterProperties.enableProperties = true;
                    propertyControl_FilterProperties.ReloadFilterProperties(selectedEvent);
                }
                if (propertyInfo.affectsFloors)
                {
                    ADOBase.editor.ApplyEventsToFloors();
                }
            }
        }

        private void SetSelected(Button button, bool selected)
        {
            button.image.color = selected ? selectedColor : unselectedColor;
            button.GetComponentInChildren<TMP_Text>().color = selected ? Color.black : Color.white;
        }

        private void UpdateButtons()
        {
            this.SetSelected(this.onButton, this.value);
            this.SetSelected(this.offButton, !this.value);
        }
    }
}
