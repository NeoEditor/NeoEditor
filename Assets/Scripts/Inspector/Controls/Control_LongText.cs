using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using ADOFAI;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Controls
{
    public class Control_LongText : ControlBase
    {
        public TMP_InputField inputField;

        public TMP_InputField.SubmitEvent onEndEdit => inputField.onEndEdit;
        public override List<Selectable> selectables => new List<Selectable> { inputField };

        public override string text
        {
            get => inputField.text;
            set => inputField.text = value;
        }

        public override void Setup(bool addListener)
        {
            NeoEditor editor = NeoEditor.Instance;
            inputField.textComponent.transform.position += new Vector3(0f, -7f, 0f);
            ColorBlock colors = inputField.colors;
            colors.selectedColor = ADOFAI.InspectorPanel.selectionColor;
            inputField.colors = colors;
            if (addListener)
            {
                inputField.onEndEdit.AddListener(
                    delegate(string s)
                    {
                        using (new SaveStateScope(editor, false, true, false))
                        {
                            inspectorPanel.selectedEvent[propertyInfo.name] = inputField.text;
                            //if (propertyInfo.affectsFloors)
                            //{
                            //	editor.ApplyEventsToFloors();
                            //}
                            //if (editor.SelectionIsSingle())
                            //{
                            //	editor.ShowEventIndicators(ADOBase.editor.selectedFloors[0]);
                            //}
                        }
                    }
                );
            }
        }

        public override void ValidateInput() { }
    }
}
