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
    public class Control_Vector2 : ControlBase
    {
        public TMP_InputField inputX;
        public TMP_InputField inputY;
        public TMP_Text unitX;
        public TMP_Text unitY;
        private Rect startRect;
        private Vector2 lastValue = Vector2.zero;

        public override List<Selectable> selectables => new List<Selectable> { inputX, inputY };

        public void Awake()
        {
            //if (rectTransform != null)
            //{
            //	startRect = rectTransform.rect;
            //}
        }

        public override string text
        {
            get => Validate(inputX, inputY).ToString();
            set
            {
                Vector2 vector = lastValue = RDUtils.StringToVector2(value);
                inputX.text = ConvertNaNToEmpty(vector.x.ToString("0.######"));
                inputY.text = ConvertNaNToEmpty(vector.y.ToString("0.######"));
            }
        }

        //public override void SetRandomLayout()
        //{
        //	RandomMode randomMode = (RandomMode)propertiesPanel.inspectorPanel.selectedEvent[propertyInfo.randModeKey];
        //	if (randomControl != null && rectTransform != null)
        //	{
        //		if (randomMode != RandomMode.None)
        //		{
        //			randomControl.gameObject.SetActive(true);
        //			rectTransform.anchorMin = rectTransform.anchorMin.WithY(0.4f);
        //			rectTransform.rect.Set(startRect.x, startRect.y + 2.4f, startRect.width, startRect.height);
        //			return;
        //		}
        //		randomControl.gameObject.SetActive(false);
        //		rectTransform.anchorMin = rectTransform.anchorMin.WithY(0f);
        //		rectTransform.rect.Set(startRect.x, startRect.y, startRect.width, startRect.height);
        //	}
        //}

        public ValueTuple<string, string> Validate(TMP_InputField x, TMP_InputField y)
        {
            Vector2 vector = new Vector2(lastValue.x, lastValue.y);
            string text = ConvertEmptyToNaN(x.text);
            string text2 = ConvertEmptyToNaN(y.text);
            float x2;
            float y2;
            if (float.TryParse(text, out x2) && float.TryParse(text2, out y2))
            {
                vector = new Vector2(x2, y2);
                vector = propertyInfo.Validate(vector, inspectorPanel.selectedEvent.isFake);
            }
            else
            {
                DataTable dataTable = new DataTable();
                try
                {
                    object dictValue = dataTable.Compute(text, "");
                    vector.x = RDEditorUtils.DecodeFloat(dictValue);
                }
                catch { }
                try
                {
                    object dictValue2 = dataTable.Compute(text2, "");
                    vector.y = RDEditorUtils.DecodeFloat(dictValue2);
                }
                catch { }
            }
            if (
                inspectorPanel.selectedEvent.eventType == LevelEventType.AddDecoration
                && propertyInfo.name == "tile"
            )
            {
                vector.x = (float)Mathf.RoundToInt(vector.x);
                vector.y = (float)Mathf.RoundToInt(vector.y);
            }
            string item = ConvertNaNToEmpty(vector.x.ToString("0.######"));
            string item2 = ConvertNaNToEmpty(vector.y.ToString("0.######"));
            return new ValueTuple<string, string>(item, item2);
        }

        public override void ValidateInput()
        {
            TMP_InputField tmp_InputField = inputX;
            TMP_InputField tmp_InputField2 = inputY;
            ValueTuple<string, string> valueTuple = Validate(inputX, inputY);
            tmp_InputField.text = valueTuple.Item1;
            tmp_InputField2.text = valueTuple.Item2;
        }

        public override void SetEnabled(bool enabled, bool shown = true)
        {
            base.SetEnabled(enabled, shown);
            Color color = enabled ? Color.gray : Color.gray.WithAlpha(0.5f);
            inputX.placeholder.GetComponent<TMP_Text>().color = color;
            inputY.placeholder.GetComponent<TMP_Text>().color = color;
        }

        public override void Setup(bool addListener)
        {
            if (addListener)
            {
                inputX.onEndEdit.AddListener(
                    (str) =>
                    {
                        SetVectorVals();
                    }
                );
                inputY.onEndEdit.AddListener(
                    (str) =>
                    {
                        SetVectorVals();
                    }
                );
            }
            if (!string.IsNullOrEmpty(propertyInfo.unit))
            {
                unitX.gameObject.SetActive(true);
                unitX.text = RDString.Get(
                    "editor.unit." + propertyInfo.unit,
                    null,
                    LangSection.Translations
                );
                unitY.gameObject.SetActive(true);
                unitY.text = RDString.Get(
                    "editor.unit." + propertyInfo.unit,
                    null,
                    LangSection.Translations
                );
            }
            ColorBlock colors = inputX.colors;
            colors.selectedColor = ADOFAI.InspectorPanel.selectionColor;
            inputX.colors = colors;
            inputY.colors = colors;
        }

        private void SetVectorVals()
        {
            NeoEditor editor = NeoEditor.Instance;
            using (new SaveStateScope(editor, false, true, false))
            {
                ValidateInput();
                LevelEvent selectedEvent = inspectorPanel.selectedEvent;
                string s = ConvertEmptyToNaN(inputX.text);
                string s2 = ConvertEmptyToNaN(inputY.text);
                float x = float.Parse(s);
                float y = float.Parse(s2);
                Vector2 vector = new Vector2(x, y);
                selectedEvent[propertyInfo.name] = vector;
                ToggleOthersEnabled();
                if (selectedEvent.eventType == LevelEventType.BackgroundSettings)
                {
                    ADOBase.customLevel.SetBackground();
                }
                else if (selectedEvent.IsDecoration)
                {
                    editor.UpdateDecorationObject(selectedEvent);
                }
                if (
                    selectedEvent.eventType == LevelEventType.PositionTrack
                    || selectedEvent.eventType == LevelEventType.FreeRoam
                    || selectedEvent.eventType == LevelEventType.FreeRoamTwirl
                    || selectedEvent.eventType == LevelEventType.FreeRoamRemove
                    || selectedEvent.eventType == LevelEventType.FreeRoamWarning
                )
                {
                    //editor.ApplyEventsToFloors();
                    //if (ADOBase.editor.SelectionIsSingle())
                    //{
                    //    ADOBase.editor.floorButtonCanvas.transform.position = ADOBase
                    //        .editor
                    //        .selectedFloors[0]
                    //        .transform
                    //        .position;
                    //}
                }
            }
        }

        private string ConvertNaNToEmpty(string s)
        {
            if (s == "NaN")
            {
                return "";
            }
            return s;
        }

        private string ConvertEmptyToNaN(string s)
        {
            if (s == "")
            {
                return "NaN";
            }
            return s;
        }
    }
}
