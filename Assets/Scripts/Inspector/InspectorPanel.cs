﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADOFAI;
using DG.Tweening;
using HarmonyLib;
using NeoEditor.Inspector.Controls;
using NeoEditor.Tabs;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class InspectorPanel : MonoBehaviour
    {
        public struct PropertySelectable
        {
            public Selectable selectable;

            public ControlBase control;

            public Property propertyRef;

            public bool isPropertyCheckbox;

            public PropertySelectable(
                Selectable sel,
                ControlBase control,
                Property propertyRef,
                bool isPropertyCheckbox = false
            )
            {
                selectable = sel;
                this.control = control;
                this.propertyRef = propertyRef;
                this.isPropertyCheckbox = isPropertyCheckbox;
            }
        }

        public RectTransform content;
        public LayoutGroup layout;
        public RectTransform viewport;

        public Dictionary<string, Property> properties = new Dictionary<string, Property>();
        public LevelEvent selectedEvent;

        public List<PropertySelectable> propertySelectables = new List<PropertySelectable>();
        public TabBase parentTab;

        private EventSystem eventSystem;

		public string selectedTab { get; set; }

		public void Init(LevelEventInfo levelEventInfo, bool addFloorControl = false)
        {
            eventSystem = EventSystem.current;
            Dictionary<string, ADOFAI.PropertyInfo> propertiesInfo = levelEventInfo.propertiesInfo;
            List<string> dontRenderKeys =
                typeof(PropertiesPanel).GetField("dontRenderKeys", AccessTools.all).GetValue(null)
                as List<string>;
            NeoEditor editor = NeoEditor.Instance;

            bool isDecoration =
                levelEventInfo.type == LevelEventType.AddDecoration
                || levelEventInfo.type == LevelEventType.AddText
                || levelEventInfo.type == LevelEventType.AddObject;

            List<string> keys;

            if (
                !isDecoration
                && addFloorControl
                && !GCS.settingsInfo.Values.Contains(levelEventInfo)
                && !propertiesInfo.Keys.Contains("floor")
            )
            {
                Dictionary<string, object> dict = new Dictionary<string, object>
                {
                    { "name", "floor" },
                    { "type", "Int" },
                    { "default", 0 },
                    { "key", "editor.tileNumber" },
                    { "canBeDisabled", false }
                };
                ADOFAI.PropertyInfo propertyInfo = new ADOFAI.PropertyInfo(dict, levelEventInfo);
                propertiesInfo.Add("floor", propertyInfo);

                keys = propertiesInfo.Keys.ToList();
                string floor = keys[keys.Count - 1];
                keys.Remove(keys[keys.Count - 1]);
                keys.Insert(0, floor);
            }
            else
                keys = propertiesInfo.Keys.ToList();

            foreach (string propertyKey in keys)
            {
                if (dontRenderKeys.Contains(propertyKey))
                    continue;

                ADOFAI.PropertyInfo propertyInfo = propertiesInfo[propertyKey];
                if (propertyInfo.controlType == ControlType.Hidden || propertyInfo.pro)
                    continue;

                GameObject original = null;
                List<string> typeNames = new List<string>();
                List<string> localizatedTypeNames = new List<string>();
                switch (propertyInfo.type)
                {
                    case PropertyType.File:
                        original = editor.prefab_controlFile;
                        break;
                    case PropertyType.Int:
                    case PropertyType.Float:
                    case PropertyType.String:
                        original = editor.prefab_controlText;
                        if (propertyInfo.stringDropdown)
                        {
                            original = editor.prefab_controlToggle;
                            string text2 = propertyInfo.name;
                            if (!(text2 == "component"))
                            {
                                if (text2 == "filter")
                                {
                                    foreach (
                                        Type item3 in from t in typeof(CameraFilterPack_AAA_SuperComputer).Assembly.GetTypes()
                                        where t.Name.StartsWith("CameraFilterPack_")
                                        select t
                                    )
                                    {
                                        typeNames.Add(item3.ToString());
                                        string text3 = item3.Name.Replace("CameraFilterPack_", "");
                                        string text4 = text3.UnCamelCase(
                                            RDUtils.UnCamelCaseOptions.SpaceBeforeNumbers
                                                | RDUtils.UnCamelCaseOptions.UnderbarToSpace
                                        );
                                        bool exists;
                                        string withCheck = RDString.GetWithCheck(
                                            "editor.CameraFilterPack." + text3,
                                            out exists
                                        );
                                        localizatedTypeNames.Add(exists ? withCheck : text4);
                                    }
                                }
                            }
                            else
                            {
                                Type parentType = typeof(ffxPlusBase);
                                Type parentType2 = typeof(ffxBase);
                                foreach (
                                    Type item4 in from t in Assembly
                                        .GetExecutingAssembly()
                                        .GetTypes()
                                    where t.IsSubclassOf(parentType) || t.IsSubclassOf(parentType2)
                                    select t
                                )
                                {
                                    typeNames.Add(item4.ToString());
                                }
                            }
                        }

                        if (propertyInfo.slider)
                        {
                            continue;
                            original = ADOBase.gc.prefab_controlSlider;
                        }

                        break;
                    case PropertyType.LongString:
                        original = editor.prefab_controlLongText;
                        break;
                    case PropertyType.Enum:
                    {
                        Type enumType = propertyInfo.enumType;
                        original = editor.prefab_controlToggle;
                        string[] names = Enum.GetNames(enumType);
                        foreach (string text in names)
                        {
                            if (
                                (
                                    enumType != typeof(Ease)
                                    || (
                                        text != "Unset"
                                        && text != "INTERNAL_Zero"
                                        && text != "INTERNAL_Custom"
                                    )
                                )
                                && (
                                    (
                                        levelEventInfo.type != LevelEventType.CameraSettings
                                        && !levelEventInfo.isDecoration
                                    ) || !(text == "LastPosition")
                                )
                            )
                            {
                                typeNames.Add(text);
                            }
                        }

                        break;
                    }
                    case PropertyType.Color:
                        original = editor.prefab_controlColor;
                        break;
                    case PropertyType.Bool:
                        original = editor.prefab_controlBool;
                        break;
                    case PropertyType.Vector2:
                        original = editor.prefab_controlVector2;
                        break;
                    case PropertyType.Tile:
                        original = editor.prefab_controlTile;
                        break;
                    case PropertyType.Export:
                        continue;
                        if (!SteamIntegration.Instance.initialized)
                        {
                            continue;
                        }

                        original = ADOBase.gc.prefab_controlExport;
                        break;
                        continue;
                    case PropertyType.Rating:
                        original = editor.prefab_controlToggle;
                        break;
                    case PropertyType.List:
                        continue;
                        if (propertyInfo.name == "decorations")
                        {
                            original = ADOBase.gc.prefab_controlDecorationsList;
                        }
                        else if (propertyInfo.name == "events")
                        {
                            original = ADOBase.gc.prefab_controlEventsList;
                        }

                        break;
                    case PropertyType.FilterProperties:
                        continue;
                        original = ADOBase.gc.prefab_controlFilterProperties;
                        break;
                }

                GameObject gameObject = Instantiate(editor.prefab_property);
                gameObject.transform.SetParent(content, worldPositionStays: false);
                PropertyPlus property = gameObject.GetComponent<PropertyPlus>();
                property.gameObject.name = propertyKey;
                property.key = propertyKey;
                property.info = propertyInfo;
                GameObject gameObject2 = Instantiate(original);
                gameObject2
                    .GetComponent<RectTransform>()
                    .SetParent(property.controlContainer, worldPositionStays: false);
                property.control = gameObject2.GetComponent<ControlBase>();
                property.control.propertyInfo = propertyInfo;
                property.control.inspectorPanel = this;
                property.control.propertyTransform = property.GetComponent<RectTransform>();

                if (propertyInfo.type == PropertyType.Enum || propertyInfo.stringDropdown)
                {
                    ((Control_Toggle)property.control).EnumSetup(
                        propertyInfo.enumTypeString,
                        typeNames,
                        propertyInfo.type == PropertyType.Enum,
                        localizatedTypeNames
                    );
                }
                //else if (propertyInfo.type == PropertyType.List && propertyKey == "decorations")
                //{
                //    if (scrollRect.gameObject.TryGetComponent<SmoothScrollRect>(out var component))
                //    {
                //        component.enabled = false;
                //    }

                //    ScrollRect component2 = gameObject2.GetComponent<ScrollRect>();
                //    component2.horizontalScrollbar = scrollRect.horizontalScrollbar;
                //    component2.verticalScrollbar = scrollRect.verticalScrollbar;
                //    scrollRect.horizontalScrollbar = null;
                //    scrollRect.verticalScrollbar = null;
                //    property.label.transform.parent.gameObject.SetActive(value: false);
                //    ADOBase.editor.decorationsListContent = component2.content;
                //    scrollRect.content = ADOBase.editor.decorationsListContent;
                //    scrollRect.vertical = false;
                //    scrollRect.viewport = gameObject2.GetComponent<ScrollRect>().viewport;
                //    RectTransform component3 =
                //        component2.verticalScrollbar.transform.parent.GetComponent<RectTransform>();
                //    component3.offsetMin = component3.offsetMin.WithY(60f);
                //    component3.offsetMax = component3.offsetMax.WithY(-50f);
                //    ADOBase.editor.propertyControlDecorationsList =
                //        (PropertyControl_DecorationsList)property.control;
                //    ((PropertyControl_List)property.control).parentReferenceRT =
                //        GetComponent<RectTransform>();
                //}
                //else if (propertyInfo.type == PropertyType.List && propertyKey == "events")
                //{
                //    ADOBase.editor.eventsListContent = gameObject2
                //        .GetComponent<ScrollRect>()
                //        .content;
                //    scrollRect.content = ADOBase.editor.eventsListContent;
                //    scrollRect.vertical = false;
                //    scrollRect.viewport = gameObject2.GetComponent<ScrollRect>().viewport;
                //    RectTransform component4 =
                //        scrollRect.verticalScrollbar.transform.parent.GetComponent<RectTransform>();
                //    component4.offsetMin = component4.offsetMin.WithY(60f);
                //    component4.offsetMax = component4.offsetMax.WithY(-50f);
                //    ADOBase.editor.propertyControlEventsList = (PropertyControl_EventsList)
                //        property.control;
                //    ((PropertyControl_List)property.control).parentReferenceRT =
                //        GetComponent<RectTransform>();
                //}
                else if (propertyInfo.type == PropertyType.String)
                {
                    Control_Text control_Text = property.control as Control_Text;
                    string key =
                        propertyInfo.placeholder
                        ?? (
                            "editor."
                            + levelEventInfo.name
                            + "."
                            + propertyInfo.name
                            + ".placeholder"
                        );
                    bool exists2 = false;
                    string withCheck2 = RDString.GetWithCheck(key, out exists2);
                    if (exists2)
                    {
                        (control_Text.inputField.placeholder as TMP_Text).text = withCheck2;
                    }
                }
                else if (propertyInfo.type == PropertyType.Int && propertyKey == "floor")
                {
                    Control_Text obj = property.control as Control_Text;
                    obj.linkFloorIDButton.gameObject.SetActive(value: true);
                    obj.goToFloorButton.gameObject.SetActive(value: true);
                    obj.inputField.characterLimit = 10;
                }
                else if (propertyInfo.type == PropertyType.Tile)
                {
                    Control_Tile obj = property.control as Control_Tile;
                    obj.inputField.linkFloorIDButton.gameObject.SetActive(value: true);
                    obj.inputField.goToFloorButton.gameObject.SetActive(value: true);
                    obj.inputField.inputField.characterLimit = 10;
                }
                else if (propertyInfo.type == PropertyType.Rating)
                {
                    Control_Toggle obj = property.control as Control_Toggle;
                    List<string> enumVals = new List<string>();
                    List<string> labels = new List<string>();
                    for (int i = 1; i <= 10; i++)
                    {
                        enumVals.Add(i.ToString());
                        string color =
                            i <= 3
                                ? "8AFFAB"
                                : i <= 6
                                    ? "FFEE6B"
                                    : i <= 9
                                        ? "FF4257"
                                        : "FD8AFF";
                        labels.Add($"<color=#{color}>☆{i}</color>");
                    }
                    obj.EnumSetup("Rating", enumVals, false, enumVals);
                }

                if (propertyInfo.type == PropertyType.Vector2)
                {
                    Control_Vector2 obj2 = property.control as Control_Vector2;
                    TMP_Text tMP_Text = obj2.inputX.placeholder as TMP_Text;
                    TMP_Text obj3 = obj2.inputY.placeholder as TMP_Text;
                    tMP_Text.text = (propertyInfo.vector2_allowEmpty ? "—" : ""); // u+2014
                    obj3.text = (propertyInfo.vector2_allowEmpty ? "—" : "");
                }

                string key2 = "editor." + property.key + ".help";
                bool exists3;
                string helpString = RDString.GetWithCheck(key2, out exists3);
                if (exists3)
                {
                    Button helpButton = property.helpButton;
                    helpButton.transform.parent.gameObject.SetActive(value: true);
                    string buttonText = RDString.GetWithCheck(
                        "editor." + property.key + ".help.buttonText",
                        out exists3
                    );
                    string buttonURL = RDString.GetWithCheck(
                        "editor." + property.key + ".help.buttonURL",
                        out exists3
                    );
                    helpButton.onClick.AddListener(
                        delegate
                        {
                            ADOBase.editor.ShowPropertyHelp(
                                show: true,
                                helpButton.transform,
                                helpString,
                                buttonText,
                                buttonURL
                            );
                        }
                    );
                }

                property.control.Setup(addListener: true);
                //if (property.info.hasRandomValue)
                //{
                //    string randValueKey = property.info.randValueKey;
                //    property.control.randomControl.propertyInfo = levelEventInfo.propertiesInfo[
                //        randValueKey
                //    ];
                //    //property.control.randomControl.propertiesPanel = this;
                //    property.control.randomControl.Setup(addListener: true);
                //    Button randomButton = property.randomButton;
                //    randomButton.gameObject.SetActive(value: true);
                //    randomButton.onClick.AddListener(
                //        delegate
                //        {
                //            string randModeKey = property.info.randModeKey;
                //            int num = ((int)selectedEvent[randModeKey] + 1) % 3;
                //            selectedEvent[randModeKey] = (RandomMode)num;
                //            property.control.SetRandomLayout();
                //        }
                //    );
                //}

                property.enabledButton.onClick.AddListener(
                    delegate
                    {
                        bool isFake = selectedEvent.isFake;
                        using (new SaveStateScope(editor))
                        {
                            bool value;
                            bool flag =
                                selectedEvent.disabled.TryGetValue(propertyKey, out value)
                                && !value;
                            selectedEvent.disabled[propertyKey] = flag;
                            property.offText.SetActive(flag);
                            //property.enabledCheckmark.SetActive(!flag);
                            property.enabledButton.image.sprite = !flag
                                ? property.checkSprite
                                : property.uncheckSprite;
                            property.control.gameObject.SetActive(!flag);
                            if (isFake)
                            {
                                //property.enabledCheckmark.transform.parent.gameObject.SetActive(
                                //    value: false
                                //);
                                property.enabledButton.gameObject.SetActive(value: false);
                                selectedEvent.ApplyPropertiesToRealEvents();
                            }

                            if (property.info.affectsFloors)
                            {
                                ADOBase.editor.ApplyEventsToFloors();
                            }
                        }
                        StartCoroutine(
                            InvokeAtNextFrame(
                                () => LayoutRebuilder.ForceRebuildLayoutImmediate(content)
                            )
                        );
                    }
                );
                if (property.info.canBeDisabled)
                {
                    Button component5 = property.enabledButton.GetComponent<Button>();
                    ColorBlock colors = component5.colors;
                    colors.selectedColor = ADOFAI.InspectorPanel.selectionColor;
                    component5.colors = colors;
                    PropertySelectable item = new PropertySelectable(
                        component5,
                        property.control,
                        property,
                        isPropertyCheckbox: true
                    );
                    propertySelectables.Add(item);
                }

                if (property.control.selectables != null)
                {
                    foreach (Selectable selectable in property.control.selectables)
                    {
                        PropertySelectable item2 = new PropertySelectable(
                            selectable,
                            property.control,
                            property
                        );
                        propertySelectables.Add(item2);
                    }
                }

                properties.Add(propertyInfo.name, property);
            }
        }

        public void SetProperties(LevelEvent levelEvent, bool checkIfEnabled = true)
        {
            selectedEvent = levelEvent;
            NeoEditor editor = NeoEditor.Instance;
            foreach (string item in levelEvent.data.Keys.ToList())
            {
                if (!properties.ContainsKey(item))
                {
                    continue;
                }

                PropertyPlus property = properties[item] as PropertyPlus;
                ControlBase control = property.control;
                if (control == null)
                {
                    continue;
                }

                PropertyType type = control.propertyInfo.type;
                if (type == PropertyType.Export || type == PropertyType.List)
                {
                    continue;
                }

                switch (type)
                {
                    case PropertyType.Vector2:
                        control.text = ((Vector2)levelEvent[item]).ToString("f6");
                        if (control.propertyInfo.hasRandomValue)
                        {
                            control.randomControl.text = (
                                (Vector2)levelEvent[control.propertyInfo.randValueKey]
                            ).ToString("f6");
                            control.SetRandomLayout();
                        }

                        break;
                    case PropertyType.Tile:
                        (control as Control_Tile).tileValue =
                            levelEvent[item] as Tuple<int, TileRelativeTo>;
                        break;
                    case PropertyType.FilterProperties:
                    {
                        //PropertyControl_FilterProperties propertyControl_FilterProperties =
                        //    control as PropertyControl_FilterProperties;
                        //if (levelEvent.data.TryGetValue("isNewlyAdded", out var value))
                        //{
                        //    propertyControl_FilterProperties.enableProperties = (bool)value;
                        //    levelEvent.data.Remove("isNewlyAdded");
                        //}

                        break;
                    }
                    case PropertyType.Bool:
                        ((Control_Bool)control).value = levelEvent.GetBool(item);
                        break;
                    default:
                    {
                        string text = levelEvent[item].ToString();
                        control.text = (
                            (control.propertyInfo.stringDropdown && string.IsNullOrEmpty(text))
                                ? ((Control_Toggle)control).dropdown.items.First().value
                                : text
                        );
                        control.text = text;
                        //if (control.propertyInfo.hasRandomValue)
                        //{
                        //    control.randomControl.text = levelEvent[
                        //        control.propertyInfo.randValueKey
                        //    ]
                        //        .ToString();
                        //    control.SetRandomLayout();
                        //}

                        break;
                    }
                }

                if (checkIfEnabled)
                {
                    control.ToggleOthersEnabled();
                }

                if (selectedEvent != null)
                {
                    SetupCheckmark(property, control);
                }

                property.control?.OnSelectedEventChanged(levelEvent);
            }

            if (levelEvent.info.propertiesInfo.ContainsKey("floor"))
            {
                int num = Mathf.Clamp(levelEvent.floor, 0, editor.floors.Count - 1);
                PropertyPlus property2 = properties["floor"] as PropertyPlus;
                ControlBase control2 = property2.control;
                control2.text = num.ToString();
                SetupCheckmark(property2, control2);
            }

            StartCoroutine(
                InvokeAtNextFrame(() => LayoutRebuilder.ForceRebuildLayoutImmediate(content))
            );
        }

        public void SetupCheckmark(PropertyPlus property, ControlBase control)
        {
            bool flag = selectedEvent?.isFake ?? false;
            bool flag2 = control.propertyInfo.canBeDisabled || flag;
            bool value = default(bool);
            bool flag3 =
                flag2
                && selectedEvent.disabled.TryGetValue(control.propertyInfo.name, out value)
                && value;
            bool active = (flag ? flag3 : flag2);
            property.offText.SetActive(flag3);
            //property.enabledCheckmark.SetActive(!flag3);
            property.enabledButton.image.sprite = !flag3
                ? property.checkSprite
                : property.uncheckSprite;
            control.gameObject.SetActive(!flag3);
            property.enabledButton.gameObject.SetActive(value: true);
            //property.enabledCheckmark.transform.parent.gameObject.SetActive(active);
            property.enabledButton.gameObject.SetActive(active);
        }

        private IEnumerator InvokeAtNextFrame(Action action)
        {
            yield return null;
            action.Invoke();
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Tab))
            {
                return;
            }
            if (
                propertySelectables == null
                || propertySelectables.Count == 0
                || eventSystem == null
                || eventSystem.currentSelectedGameObject == null
            )
            {
                return;
            }

            Selectable currentSel =
                eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
            int num = propertySelectables.FindIndex(
                (PropertySelectable ps) => ps.selectable == currentSel
            );
            if (currentSel == null || num < 0)
            {
                return;
            }

            bool holdingControl = RDInput.holdingControl;
            bool holdingShift = RDInput.holdingShift;
            int num2 = num;
            int num3 = 0;
            bool flag;
            PropertySelectable propertySelectable;
            ControlBase control;
            bool flag2;
            do
            {
                flag = false;
                flag2 = false;
                num2 = ((holdingControl || holdingShift) ? (num2 - 1) : (num2 + 1));
                num2 = (
                    (num2 >= 0)
                        ? (num2 % propertySelectables.Count)
                        : (propertySelectables.Count - 1)
                );
                propertySelectable = propertySelectables[num2];
                control = propertySelectable.control;
                flag2 = propertySelectable.isPropertyCheckbox;
                if (control.propertyInfo.canBeDisabled || selectedEvent.isFake)
                {
                    flag = selectedEvent.disabled[control.propertyInfo.name];
                }

                num3++;
                if (num3 > 999)
                {
                    num2 = num;
                    break;
                }
            } while (!control.propertyInfo.CheckIfEnabled(selectedEvent, selectedTab) || (flag && !flag2));
            Selectable selectable = propertySelectables[num2].selectable;
            if (flag2)
            {
                selectable.targetGraphic = (
                    flag
                        ? propertySelectable.propertyRef.checkbgImage
                        : propertySelectable.propertyRef.checkImage
                );
            }

            RectTransform propertyTransform = control.propertyTransform;
            if (!IsControlFullyVisible(propertyTransform))
            {
                float num4 = content.anchoredPosition.y;
                float num5 = propertyTransform.anchoredPosition.y;
                float num6 = (
                    (num5 > 0f - num4)
                        ? num5
                        : (num5 - propertyTransform.rect.height + viewport.rect.height)
                );
                content.SetAnchorPosY(0f - num6);
            }

            if (selectable.TryGetComponent<InputField>(out var component))
            {
                component.OnPointerClick(new PointerEventData(eventSystem));
            }
            eventSystem.SetSelectedGameObject(
                selectable.gameObject,
                new BaseEventData(eventSystem)
            );
        }

        private bool IsControlFullyVisible(RectTransform rt)
        {
            float y = content.anchoredPosition.y;
            float y2 = rt.anchoredPosition.y;
            float height = rt.rect.height;
            float height2 = this.viewport.rect.height;
            return y2 <= -y && y2 - height > -height2 - y;
        }
    }
}
