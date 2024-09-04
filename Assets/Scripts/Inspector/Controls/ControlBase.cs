using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using ADOFAI.LevelEditor.Controls;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Controls
{
    public class ControlBase : MonoBehaviour
    {
        public PropertyInfo propertyInfo;
        public Inspector.InspectorPanel inspectorPanel;
        public RectTransform propertyTransform;
        public PropertyControl randomControl;

        public virtual List<Selectable> selectables
        {
            get => null;
            set { }
        }
        public virtual string text
        {
            get => null;
            set { }
        }

        public virtual void OnRightClick() { }

        public virtual void ValidateInput() { }

        public virtual void Setup(bool addListener) { }

        public virtual void EnumSetup(
            string enumTypeString,
            List<string> enumVals,
            bool localize = true,
            List<string> customLabels = null
        ) { }

        public void ToggleOthersEnabled()
        {
            if (inspectorPanel.name == "LevelSettings")
            {
                //PropertyPlus property = inspectorPanel.properties["specialArtistType"] as PropertyPlus;
                //ApprovalLevelBadge approvalLevelBadge = ADOBase
                //    .editor
                //    .settingsPanel
                //    .approvalLevelBadge;
                //bool enabled =
                //    approvalLevelBadge == null
                //    || approvalLevelBadge.approvalLevel == ApprovalLevel.Pending;
                //property.control.SetEnabled(enabled, true);
                //PropertyPlus property2 = inspectorPanel.properties["artistPermission"] as PropertyPlus;
                //bool enabled2 = false;
                //if (approvalLevelBadge != null)
                //{
                //    if (approvalLevelBadge.approvalLevel == ApprovalLevel.Pending)
                //    {
                //        enabled2 = (
                //            (SpecialArtistType)
                //                ADOBase.editor.settingsPanel.selectedEvent["specialArtistType"]
                //            == SpecialArtistType.None
                //        );
                //    }
                //}
                //else
                //{
                //    enabled2 = true;
                //}
                //property2.control.SetEnabled(enabled2, true);
            }
            foreach (PropertyPlus property3 in inspectorPanel.properties.Values)
            {
                if (
                    property3.info.name != "specialArtistType"
                    && property3.info.name != "artistPermission"
                )
                {
                    property3.control.UpdateEnabled();
                }
            }
        }

        public void UpdateEnabled()
        {
            bool enabled = propertyInfo.CheckIfEnabled(inspectorPanel.selectedEvent, inspectorPanel.selectedTab);
            bool shown = propertyInfo.CheckIfShown(inspectorPanel.selectedEvent, inspectorPanel.selectedTab);
            SetEnabled(enabled, shown);
        }

        public virtual void SetEnabled(bool enabled, bool shown = true)
        {
            //Color color = enabled ? Color.white : Color.gray;
            //foreach (TMP_Text tmp_Text in base.GetComponentsInChildren<TMP_Text>())
            //{
            //    if (tmp_Text.color != Color.black)
            //    {
            //        tmp_Text.color = color;
            //    }
            //}
            Selectable[] componentsInChildren2 = base.GetComponentsInChildren<Selectable>();
            for (int i = 0; i < componentsInChildren2.Length; i++)
            {
                componentsInChildren2[i].interactable = enabled;
            }
            Slider[] componentsInChildren3 = base.GetComponentsInChildren<Slider>();
            for (int i = 0; i < componentsInChildren3.Length; i++)
            {
                Image component = componentsInChildren3[i]
                    .transform.GetChild(0)
                    .GetChild(0)
                    .GetComponent<Image>();
                component.color = component.color.WithAlpha(enabled ? 1f : 0.5f);
            }
            propertyInfo.isEnabled = enabled;
            SetShown(shown);
        }

        public void SetShown(bool shown)
        {
            if (!propertyInfo.invisible)
            {
                bool flag = propertyTransform.gameObject.activeSelf != shown;
            }
            propertyTransform.gameObject.SetActive(shown);
        }

        public virtual void SetRandomLayout() { }

        public virtual void OnSelectedEventChanged(LevelEvent levelEvent) { }
    }
}
