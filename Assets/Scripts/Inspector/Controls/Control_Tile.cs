using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ADOFAI;
using SA.GoogleDoc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Controls
{
    public class Control_Tile : ControlBase
    {
        public Control_Text inputField;
        public Control_Toggle buttonsToggle;

        public override List<Selectable> selectables =>
            inputField.selectables.Concat(buttonsToggle.selectables).ToList<Selectable>();

        public Tuple<int, TileRelativeTo> tileValue
        {
            get
            {
                int item = Convert.ToInt32(inputField.text);
                TileRelativeTo item2 = RDUtils.ParseEnum<TileRelativeTo>(
                    buttonsToggle.text,
                    TileRelativeTo.ThisTile
                );
                return new Tuple<int, TileRelativeTo>(item, item2);
            }
            set
            {
                inputField.text = value.Item1.ToString();
                buttonsToggle.text = value.Item2.ToString();
            }
        }

        public override void Setup(bool addListener)
        {
            inputField.propertyInfo = propertyInfo;
            inputField.inspectorPanel = inspectorPanel;
            buttonsToggle.propertyInfo = propertyInfo;
            buttonsToggle.inspectorPanel = inspectorPanel;
            inputField.Setup(addListener);
            List<string> list = new List<string>();
            foreach (object obj in Enum.GetValues(typeof(TileRelativeTo)))
            {
                list.Add(((TileRelativeTo)obj).ToString());
            }
            List<string> enumString = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                enumString.Add(
                    RDString.Get("enum.TileRelativeTo." + list[i], null, LangSection.Translations)
                );
            }
            buttonsToggle.EnumSetup(nameof(TileRelativeTo), list, false, enumString);
        }
    }
}
