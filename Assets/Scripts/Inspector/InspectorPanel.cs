using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using HarmonyLib;
using NeoEditor.Inspector.Controls;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace NeoEditor.Inspector
{
    public class InspectorPanel : MonoBehaviour
    {
        public RectTransform content;

        public Dictionary<string, Property> properties = new Dictionary<string, Property>();
        public LevelEvent selectedEvent;

        public void Init(LevelEventInfo levelEventInfo)
        {
            Dictionary<string, PropertyInfo> propertiesInfo = levelEventInfo.propertiesInfo;
            using (var enumerator = propertiesInfo.Keys.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    NeoEditor editor = NeoEditor.Instance;
                    string propertyKey = enumerator.Current;
                    var dontRenderKeys =
                        (List<string>)
                            typeof(PropertiesPanel)
                                .GetField("dontRenderKeys", AccessTools.all)
                                .GetValue(null);
                    if (dontRenderKeys.Contains(propertyKey))
                        continue;

                    PropertyInfo propertyInfo = propertiesInfo[propertyKey];
                    if (propertyInfo.controlType == ControlType.Hidden || propertyInfo.pro)
                        continue;

                    GameObject prefab;
                    List<string> enumNames = new List<string>();
                    List<string> enumLocalizations = new List<string>();

                    switch (propertyInfo.type)
                    {
                        case PropertyType.Bool:
                            prefab = editor.prefab_controlBool;
                            break;
                        case PropertyType.Int:
                        case PropertyType.Float:
                        case PropertyType.String:
                            prefab = editor.prefab_controlText;
                            if (propertyInfo.stringDropdown)
                            {
                                //prefab = ADOBase.gc.prefab_controlToggle;
                                //string name = propertyInfo.name;
                                //if (name != "component")
                                //{
                                //	if (name != "filter")
                                //	{
                                //		if (propertyInfo.slider)
                                //		{
                                //			prefab = ADOBase.gc.prefab_controlSlider;
                                //		}
                                //	}
                                //}
                                //foreach (System.Type type2 in from t in typeof(CameraFilterPack_AAA_SuperComputer).Assembly.GetTypes()
                                //					   where t.Name.StartsWith("CameraFilterPack_")
                                //					   select t)
                                //{
                                //	list.Add(type2.ToString());
                                //	string text = type2.Name.Replace("CameraFilterPack_", "");
                                //	string text2 = text.UnCamelCase(RDUtils.UnCamelCaseOptions.SpaceBeforeNumbers | RDUtils.UnCamelCaseOptions.UnderbarToSpace);
                                //	bool flag;
                                //	string withCheck = RDString.GetWithCheck("editor.CameraFilterPack." + text, out flag, null, LangSection.Translations);
                                //	list2.Add(flag ? withCheck : text2);
                                //}
                            }
                            break;
                        case PropertyType.LongString:
                            prefab = editor.prefab_controlLongText;
                            break;
                        case PropertyType.Color:
                            prefab = editor.prefab_controlColor;
                            break;
                        case PropertyType.File:
                            prefab = editor.prefab_controlFile;
                            break;
                        default:
                            prefab = editor.prefab_controlBool;
                            break;
                        //continue;
                    }
                    GameObject gameObject = Instantiate(editor.prefab_property);
                    gameObject.transform.SetParent(content, false);
                    Property property = gameObject.GetComponent<Property>();

                    property.gameObject.name = propertyKey;
                    property.key = propertyKey;
                    property.info = propertyInfo;
                    GameObject gameObject2 = Instantiate(prefab);
                    gameObject2
                        .GetComponent<RectTransform>()
                        .SetParent(property.controlContainer, false);

                    ControlBase control = gameObject2.GetComponent<ControlBase>();
                    control.propertyInfo = propertyInfo;
                    control.inspectorPanel = this;
                    control.propertyTransform = property.GetComponent<RectTransform>();

                    control.Setup(true);
                }
            }
        }

        void Update() { }
    }
}
