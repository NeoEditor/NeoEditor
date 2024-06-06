using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ADOFAI;
using DG.Tweening;
using HarmonyLib;
using NeoEditor.Inspector.Controls;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
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

        public Dictionary<string, Property> properties = new Dictionary<string, Property>();
        public LevelEvent selectedEvent;

        public List<PropertySelectable> propertySelectables = new List<PropertySelectable>();

        public void Init(LevelEventInfo levelEventInfo)
        {
            Dictionary<string, ADOFAI.PropertyInfo> propertiesInfo = levelEventInfo.propertiesInfo;
            List<string> dontRenderKeys =
                typeof(PropertiesPanel).GetField("dontRenderKeys", AccessTools.all).GetValue(null)
                as List<string>;
            NeoEditor editor = NeoEditor.Instance;

            foreach (string propertyKey in propertiesInfo.Keys)
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
                            continue;
                            original = ADOBase.gc.prefab_controlToggle;
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
                        continue;
                        Type enumType = propertyInfo.enumType;
                        original = ADOBase.gc.prefab_controlToggle;
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
                        continue;
                        original = ADOBase.gc.prefab_controlVector2;
                        break;
                    case PropertyType.Tile:
                        continue;
                        original = ADOBase.gc.prefab_controlTile;
                        break;
                    case PropertyType.Export:
                        //if (!SteamIntegration.Instance.initialized)
                        //{
                        //    continue;
                        //}

                        //original = ADOBase.gc.prefab_controlExport;
                        //break;
                        continue;
                    case PropertyType.Rating:
                        continue;
                        original = ADOBase.gc.prefab_controlRating;
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
                //property.control = gameObject2.GetComponent<PropertyControl>();
                //property.control.propertyInfo = propertyInfo;
                //property.control.propertiesPanel = this;
                //property.control.propertyTransform = property.GetComponent<RectTransform>();

                property.control = gameObject2.GetComponent<ControlBase>();
                property.control.propertyInfo = propertyInfo;
                property.control.inspectorPanel = this;
                property.control.propertyTransform = property.GetComponent<RectTransform>();

                if (propertyInfo.type == PropertyType.Enum || propertyInfo.stringDropdown)
                {
                    //((PropertyControl_Toggle)property.control).EnumSetup(
                    //    propertyInfo.enumTypeString,
                    //    typeNames,
                    //    propertyInfo.type == PropertyType.Enum,
                    //    localizatedTypeNames
                    //);
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

                if (propertyInfo.type == PropertyType.Vector2)
                {
                    //PropertyControl_Vector2 obj2 = property.control as PropertyControl_Vector2;
                    //TMP_Text tMP_Text = obj2.inputX.placeholder as TMP_Text;
                    //TMP_Text obj3 = obj2.inputY.placeholder as TMP_Text;
                    //tMP_Text.text = (propertyInfo.vector2_allowEmpty ? "�" : "");
                    //obj3.text = (propertyInfo.vector2_allowEmpty ? "�" : "");
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

                //property.control.Setup(addListener: true);
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
                            property.enabledCheckmark.SetActive(!flag);
                            property.control.gameObject.SetActive(!flag);
                            property.enabledButton.GetComponent<RectTransform>().offsetMin =
                                new Vector2(0f, flag ? 0f : property.controlContainer.rect.height);
                            if (isFake)
                            {
                                property.enabledCheckmark.transform.parent.gameObject.SetActive(
                                    value: false
                                );
                                property.enabledButton.gameObject.SetActive(value: false);
                                selectedEvent.ApplyPropertiesToRealEvents();
                            }

                            if (property.info.affectsFloors)
                            {
                                ADOBase.editor.ApplyEventsToFloors();
                            }
                        }
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
            this.selectedEvent = levelEvent;
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
                        //(control as PropertyControl_Tile).tileValue =
                        //    levelEvent[item] as Tuple<int, TileRelativeTo>;
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
                        //control.text = (
                        //(control.propertyInfo.stringDropdown && string.IsNullOrEmpty(text))
                        //    ? ((PropertyControl_Toggle)control).dropdown.items.First().value
                        //: text
                        //);
                        control.text = text;
                        if (control.propertyInfo.hasRandomValue)
                        {
                            control.randomControl.text = levelEvent[
                                control.propertyInfo.randValueKey
                            ]
                                .ToString();
                            control.SetRandomLayout();
                        }

                        break;
                    }
                }

                if (checkIfEnabled)
                {
                    control.ToggleOthersEnabled();
                }

                if (selectedEvent != null)
                {
                    //SetupCheckmark(property, control);
                }

                property.control?.OnSelectedEventChanged(levelEvent);
            }

            if (levelEvent.info.propertiesInfo.ContainsKey("floor"))
            {
                int num = Mathf.Clamp(levelEvent.floor, 0, ADOBase.editor.floors.Count - 1);
                Property property2 = properties["floor"];
                PropertyControl control2 = property2.control;
                control2.text = num.ToString();
                //SetupCheckmark(property2, control2);
            }
        }

        void Update() { }
    }
}
