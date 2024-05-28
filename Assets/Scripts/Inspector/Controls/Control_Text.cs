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
    public class Control_Text : ControlBase
    {
        public TMP_InputField inputField;
        public TMP_Text unit;

        public Button goToFloorButton;
        public Button linkFloorIDButton;

        public PropertyControl parentControl;

        public TMP_InputField.SubmitEvent onEndEdit => inputField.onEndEdit;
        public override List<Selectable> selectables => new List<Selectable> { inputField };

        private void Awake()
        {
            if (linkFloorIDButton == null)
            {
                return;
            }
            goToFloorButton.onClick.AddListener(
                delegate()
                {
                    GoToFloorButton();
                }
            );
            linkFloorIDButton.onClick.AddListener(
                delegate()
                {
                    SelectFloorIDToggle();
                }
            );
        }

        public override string text
        {
            get { return Validate(); }
            set { inputField.text = value; }
        }

        //public override void SetRandomLayout()
        //{
        //	RandomMode randomMode = (RandomMode)propertiesPanel.inspectorPanel.selectedEvent[propertyInfo.randModeKey];
        //	if (randomControl != null && rectTransform != null)
        //	{
        //		if (randomMode != RandomMode.None)
        //		{
        //			randomControl.gameObject.SetActive(true);
        //			rectTransform.anchorMax = rectTransform.anchorMax.WithX(0.48f);
        //			return;
        //		}
        //		randomControl.gameObject.SetActive(false);
        //		rectTransform.anchorMax = rectTransform.anchorMax.WithX(1f);
        //	}
        //}

        private void Update()
        {
            if (linkFloorIDButton == null)
            {
                return;
            }
            if (!linkFloorIDButton.gameObject.activeSelf)
            {
                return;
            }
            Color editingColor = NeoEditor.Instance.editingColor;
            Color defaultButtonColor = NeoEditor.Instance.defaultButtonColor;
            //linkFloorIDButton.image.color = (NeoEditor.Instance.selectingFloorID ? editingColor : defaultButtonColor);
        }

        public string Validate()
        {
            if (propertyInfo == null)
            {
                return inputField.text;
            }
            if (propertyInfo.type == PropertyType.Float)
            {
                float value = 1f;
                if (float.TryParse(inputField.text, out value))
                {
                    value = propertyInfo.Validate(value);
                }
                else
                {
                    DataTable dataTable = new DataTable();
                    try
                    {
                        object dictValue = dataTable.Compute(inputField.text, "");
                        value = propertyInfo.Validate(RDEditorUtils.DecodeFloat(dictValue));
                    }
                    catch
                    {
                        value = (float)propertyInfo.value_default;
                    }
                }
                return value.ToString();
            }
            if (propertyInfo.type == PropertyType.Int || propertyInfo.type == PropertyType.Tile)
            {
                int value2 = 1;
                float f;
                if (float.TryParse(inputField.text, out f))
                {
                    value2 = Mathf.RoundToInt(f);
                    value2 = propertyInfo.Validate(value2);
                }
                else
                {
                    DataTable dataTable2 = new DataTable();
                    try
                    {
                        value2 = RDEditorUtils.DecodeInt(dataTable2.Compute(inputField.text, ""));
                    }
                    catch
                    {
                        value2 = (int)propertyInfo.value_default;
                    }
                }
                return value2.ToString();
            }
            return inputField.text;
        }

        public override void ValidateInput()
        {
            inputField.text = Validate();
        }

        public override void Setup(bool addListener)
        {
            ColorBlock colors = inputField.colors;
            colors.selectedColor = ADOFAI.InspectorPanel.selectionColor;
            inputField.colors = colors;
            if (addListener)
            {
                if (propertyInfo.name == "artist")
                {
                    inputField.onValueChanged.AddListener(
                        delegate(string s)
                        {
                            //ADOBase.editor.settingsPanel.ToggleArtistPopup(s, rectTransform.position.y, this);
                            //base.ToggleOthersEnabled();
                        }
                    );
                }
                inputField.onEndEdit.AddListener(
                    delegate(string s)
                    {
                        if (propertyInfo.name == "artist")
                        {
                            return;
                        }
                        NeoEditor editor = NeoEditor.Instance;
                        using (new SaveStateScope(editor, false, true, false))
                        {
                            ValidateInput();
                            LevelEvent selectedEvent = inspectorPanel.selectedEvent;
                            PropertyType type = propertyInfo.type;
                            string text = inputField.text;
                            string name = propertyInfo.name;
                            object obj = null;
                            switch (type)
                            {
                                case PropertyType.Int:
                                    obj = int.Parse(text);
                                    break;
                                case PropertyType.Float:
                                    obj = float.Parse(text);
                                    break;
                                case PropertyType.String:
                                    obj = text;
                                    break;
                                default:
                                    if (type == PropertyType.Tile)
                                    {
                                        Tuple<int, TileRelativeTo> tuple =
                                            selectedEvent[name] as Tuple<int, TileRelativeTo>;
                                        obj = new Tuple<int, TileRelativeTo>(
                                            int.Parse(text),
                                            tuple.Item2
                                        );
                                    }
                                    break;
                            }
                            if (name == "floor")
                            {
                                selectedEvent.floor = (int)obj;
                            }
                            else if (
                                propertyInfo.name == "angleOffset"
                                && selectedEvent.eventType == LevelEventType.SetSpeed
                            )
                            {
                                //double num = (double)((float)ADOBase.editor.selectedFloors[0].entryangle);
                                //float num2 = (float)ADOBase.editor.selectedFloors[0].exitangle;
                                //float num3 = Mathf.Round((float)scrMisc.GetAngleMoved(num, (double)num2, !ADOBase.editor.selectedFloors[0].isCCW) * 57.29578f);
                                //if (num3 <= Mathf.Pow(10f, -6f) && ADOBase.lm.leveldata[selectedEvent.floor] != '!')
                                //{
                                //	num3 = 360f;
                                //}
                                //obj = Mathf.Clamp((float)obj, 0f, num3);
                                //selectedEvent[propertyInfo.name] = obj;
                                //editor.levelEventsPanel.ShowPanelOfEvent(selectedEvent);
                            }
                            else
                            {
                                selectedEvent[name] = obj;
                            }
                            base.ToggleOthersEnabled();
                            if (propertyInfo.slider)
                            {
                                ((PropertyControl_Slider)parentControl).UpdateSliderValue(obj);
                            }
                            if (selectedEvent.eventType == LevelEventType.BackgroundSettings)
                            {
                                ADOBase.customLevel.SetBackground();
                            }
                            else if (selectedEvent.IsDecoration)
                            {
                                editor.UpdateDecorationObject(selectedEvent);
                            }
                            //if (propertyInfo.affectsFloors)
                            //{
                            //	editor.ApplyEventsToFloors();
                            //}
                            //if (editor.SelectionIsSingle())
                            //{
                            //	editor.ShowEventIndicators(editor.selectedFloors[0]);
                            //}
                        }
                    }
                );
            }
            if (!string.IsNullOrEmpty(propertyInfo.unit))
            {
                unit.gameObject.SetActive(true);
                unit.text = RDString.Get(
                    "editor.unit." + propertyInfo.unit,
                    null,
                    LangSection.Translations
                );
            }
        }

        private void GoToFloorButton()
        {
            int index;
            if (int.TryParse(text, out index))
            {
                //NeoEditor.Instance.SelectFloor(NeoEditor.Instance.floors[index], true);
            }
        }

        private void SelectFloorIDToggle()
        {
            //NeoEditor.Instance.selectingFloorID = !NeoEditor.Instance.selectingFloorID;
            //NeoEditor.Instance.selectingFloorIDTextMoving = false;
        }
    }
}
