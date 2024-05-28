using ADOFAI;
using SA.GoogleDoc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Controls
{
    public class Control_Color : ControlBase
    {
		public TMP_InputField inputField;
		public Button picker;

		public override List<Selectable> selectables
			=> new List<Selectable> { inputField };

		public override string text
		{
			get => Validate();
			set
			{
				inputField.text = value;
				picker.image.color = value.HexToColor();
			}
		}

		public string Validate()
		{
			string text = inputField.text;
			if (propertyInfo == null)
			{
				return text;
			}
			if (propertyInfo.type != PropertyType.Color)
			{
				Debug.LogError("propertyInfo.type is not PropertyType.Color");
				return "";
			}
			if (!RDUtils.IsHex(text) || text.Length < 6)
			{
				return (string)propertyInfo.value_default;
			}
			int length = 6;
			if (text.Length >= 8)
			{
				length = 8;
			}
			return text.Substring(0, length);
		}

		public override void ValidateInput()
		{
			inputField.text = Validate();
		}

		public override void Setup(bool addListener)
		{
			if (addListener)
			{
				inputField.onEndEdit.AddListener(delegate (string s)
				{
					OnEndEdit(s);
				});
				picker.onClick.AddListener(() => ShowColorPicker());
			}
			ColorBlock colors = inputField.colors;
			colors.selectedColor = ADOFAI.InspectorPanel.selectionColor;
			inputField.colors = colors;
		}

		public void ShowColorPicker()
		{
			//ADOBase.editor.colorPickerPopup.Show(this);
		}

		public void OnEndEdit(string s)
		{
			NeoEditor editor = NeoEditor.Instance;
			using (new SaveStateScope(editor, false, true, false))
			{
				ValidateInput();
				LevelEvent selectedEvent = inspectorPanel.selectedEvent;
				PropertyType type = propertyInfo.type;
				string text = inputField.text;
				selectedEvent[propertyInfo.name] = text;
				picker.image.color = text.HexToColor();
				if (selectedEvent.eventType == LevelEventType.BackgroundSettings)
				{
					ADOBase.customLevel.SetBackground();
				}
				else if (selectedEvent.IsDecoration)
				{
					editor.UpdateDecorationObject(selectedEvent);
				}
				if (propertyInfo.affectsFloors)
				{
					//editor.ApplyEventsToFloors();
				}
			}
		}
	}
}
