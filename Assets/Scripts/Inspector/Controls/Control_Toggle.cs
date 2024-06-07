using System;
using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Controls
{
    public class Control_Toggle : ControlBase
    {
        public GameObject enumButtonPrefab;
        public GameObject dropdownPrefab;
        public TweakableDropdown dropdown;
        public Image dropdownArrow;

        public Sprite selectedToggleBackground;
        public Sprite unselectedToggleBackground;

        public List<string> enumValList = new List<string>();
        public Dictionary<string, Button> buttonsDict;

        [NonSerialized]
        public List<Selectable> selectablesList = new List<Selectable>();
        public string selected;

        public bool settingText;

        //private bool useButtons =>
        //    (enumValList.Count < 3 && !propertyInfo.stringDropdown)
        //    || propertyInfo.type == PropertyType.Tile;
        private bool useButtons => false;

        public override List<Selectable> selectables
        {
            get
            {
                if (!useButtons)
                {
                    return new List<Selectable> { dropdown.inputField };
                }
                return selectablesList;
            }
        }

        public override string text
        {
            get => selected;
            set
            {
                if (useButtons)
                {
                    using (
                        Dictionary<string, Button>.KeyCollection.Enumerator enumerator =
                            buttonsDict.Keys.GetEnumerator()
                    )
                    {
                        while (enumerator.MoveNext())
                        {
                            string text = enumerator.Current;
                            SetButtonEnabled(buttonsDict[text], text == value);
                        }
                        return;
                    }
                }
                settingText = true;
                List<TweakableDropdownItem> items = dropdown.items;
                int index = enumValList.IndexOf(value);
                TweakableDropdownItem targetItem = items[index];
                dropdown.SelectItem(targetItem);
                settingText = false;
            }
        }

        public override void EnumSetup(
            string enumTypeString,
            List<string> enumVals,
            bool localize = true,
            List<string> customLabels = null
        )
        {
            int? num = (customLabels != null) ? new int?(customLabels.Count) : null;
            int count = enumVals.Count;
            bool flag = num.GetValueOrDefault() == count & num != null;
            enumValList = enumVals;
            dropdown = UnityEngine
                .Object.Instantiate<GameObject>(dropdownPrefab)
                .GetComponent<TweakableDropdown>();
            ((RectTransform)dropdown.transform).SetParent(base.transform, false);
            dropdown.gameObject.SetActive(true);
            dropdownPrefab.SetActive(false);
            dropdownArrow = dropdown.dropdownButton.transform.GetChild(0).GetComponent<Image>();
            dropdown.enumTypeString = enumTypeString;
            dropdown.localizeEnumStrings = localize;
            dropdown.useCustomLabels = (!localize && flag);
            dropdown.customLabels = customLabels;
            if (useButtons)
            {
                base.gameObject.AddComponent<HorizontalLayoutGroup>();
                dropdown.gameObject.SetActive(false);
                buttonsDict = new Dictionary<string, Button>();
                float num2 = 0f;
                int num3 = 250 / enumValList.Count;
                for (int i = 0; i < enumValList.Count; i++)
                {
                    string enumVar = enumValList[i];
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(
                        enumButtonPrefab,
                        base.transform
                    );
                    TMP_Text componentInChildren = gameObject.GetComponentInChildren<TMP_Text>();
                    if (localize)
                    {
                        componentInChildren.text = RDString.GetEnumValue(enumTypeString, enumVar);
                    }
                    else if (flag)
                    {
                        componentInChildren.text = customLabels[i];
                    }
                    else
                    {
                        componentInChildren.text = enumVar;
                    }
                    RectTransform component = gameObject.GetComponent<RectTransform>();
                    component.AnchorPosX(num2);
                    component.sizeDelta = new Vector2((float)num3, component.sizeDelta.y);
                    num2 += (float)num3;
                    Button component2 = gameObject.GetComponent<Button>();
                    buttonsDict.Add(enumVar, component2);
                    selectablesList.Add(component2);
                    component2.onClick.AddListener(
                        delegate()
                        {
                            SelectVar(enumVar);
                        }
                    );
                }
            }
            else
            {
                dropdown.itemValues.Clear();
                dropdown.itemValues.AddRange(enumValList);
                dropdown.ReloadList();
                for (int j = 0; j < dropdown.itemValues.Count; j++)
                {
                    string value = enumVals[j];
                    dropdown.items[j].value = value;
                }
                dropdown.onValueChanged = (TweakableDropdownItem selectedItem) =>
                    SelectVar(enumVals[selectedItem.index]);
            }
            ColorBlock colors = dropdown.inputField.colors;
            colors.selectedColor = ADOFAI.InspectorPanel.selectionColor;
            dropdown.inputField.colors = colors;
            foreach (Selectable selectable in selectablesList)
            {
                selectable.colors = colors;
            }
        }

        public override void Setup(bool addListener) { }

        private void Update()
        {
            if (dropdownArrow != null)
            {
                dropdownArrow.color = Color.white.WithAlpha(dropdown.interactable ? 1f : 0.3f);
            }
        }

        public void SelectVar(string var)
        {
            if (settingText)
                return;

            NeoEditor editor = NeoEditor.Instance;
            using (new SaveStateScope(editor, false, true, false))
            {
                if (buttonsDict != null && buttonsDict.Count > 0)
                {
                    foreach (string text in buttonsDict.Keys)
                    {
                        SetButtonEnabled(buttonsDict[text], text == var);
                    }
                }
                LevelEvent selectedEvent = inspectorPanel.selectedEvent;
                selected = var;
                Type enumType = propertyInfo.enumType;
                if (propertyInfo.type == PropertyType.Tile)
                {
                    Tuple<int, TileRelativeTo> tuple =
                        selectedEvent[propertyInfo.name] as Tuple<int, TileRelativeTo>;
                    selectedEvent[propertyInfo.name] = new Tuple<int, TileRelativeTo>(
                        tuple.Item1,
                        (TileRelativeTo)Enum.Parse(enumType, var)
                    );
                }
                else if (propertyInfo.stringDropdown)
                {
                    selectedEvent[propertyInfo.name] = var;
                }
                else
                {
                    selectedEvent[propertyInfo.name] = Enum.Parse(enumType, var);
                }
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
                    //PropertyControl_FilterProperties propertyControl_FilterProperties = propertiesPanel.properties["filterProperties"].control as PropertyControl_FilterProperties;
                    //propertyControl_FilterProperties.enableProperties = true;
                    //propertyControl_FilterProperties.ReloadFilterProperties(selectedEvent);
                }
                if (propertyInfo.affectsFloors)
                {
                    //editor.ApplyEventsToFloors();
                }
            }
        }

        public void SetButtonEnabled(Button button, bool enabled)
        {
            button.image.sprite = (enabled ? selectedToggleBackground : unselectedToggleBackground);
            button.GetComponentInChildren<TMP_Text>().color = (enabled ? Color.black : Color.white);
        }

        public override void OnSelectedEventChanged(LevelEvent levelEvent)
        {
            if (useButtons)
            {
                return;
            }
            if (dropdown.arrowSelectedDropdownItems.Count == 0)
            {
                return;
            }
            for (int i = 0; i < dropdown.arrowSelectedDropdownItems.Count; i++)
            {
                TweakableDropdownItem tweakableDropdownItem = dropdown.arrowSelectedDropdownItems[
                    i
                ];
                if (!(tweakableDropdownItem == dropdown.selectedItem))
                {
                    tweakableDropdownItem.OnArrowSelect(false);
                }
            }
        }
    }
}
