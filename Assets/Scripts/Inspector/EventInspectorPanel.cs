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
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoEditor.Inspector
{
    public class EventInspectorPanel : PropertiesPanel
    {
		public void Init(List<LevelEventType> events)
        {
            NeoEditor editor = NeoEditor.Instance;
            foreach (LevelEventType type in events)
            {
				if (NeoConstants.SelectorIgnoreEvents.Contains(type)
					|| NeoConstants.TimelineIgnoreEvents.Contains(type))
					continue;

				LevelEventInfo info = GCS.levelEventsInfo[type.ToString()];
				if (info != null && info.pro) continue;

				CreateEventButton button = Instantiate(editor.prefab_eventButton, content)
				.GetComponent<CreateEventButton>();
				button.button.onClick.AddListener(() =>
				{
					NeoEditor.Instance.AddEvent(type);
				});
                button.label.text = type == LevelEventType.None
					? ""
					: RDString.Get("editor." + type.ToString(), null, LangSection.Translations);
                button.icon.sprite = GCS.levelEventIcons[type];
			}
        }
    }
}
