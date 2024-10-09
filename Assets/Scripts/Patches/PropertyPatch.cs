using ADOFAI;
using ADOFAI.Editor;
using ADOFAI.LevelEditor.Controls;
using HarmonyLib;
using SA.GoogleDoc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace NeoEditor.Patches
{
	public static class PropertyPatch
	{
		private static IEnumerator InvokeAtNextFrame(Action action)
		{
			yield return null;
			action.Invoke();
		}

		[HarmonyPatch(typeof(PropertyControl_Tile), "Setup")]
		public static class DropdownTileControl
		{
			public static bool Prefix(PropertyControl_Tile __instance, bool addListener)
			{
				if (NeoEditor.Instance == null) return true;
				__instance.inputField.propertyInfo = __instance.propertyInfo;
				__instance.inputField.propertiesPanel = __instance.propertiesPanel;
				__instance.buttonsToggle.propertyInfo = __instance.propertyInfo;
				__instance.buttonsToggle.propertiesPanel = __instance.propertiesPanel;
				__instance.inputField.Setup(addListener);
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
				__instance.buttonsToggle.EnumSetup(nameof(TileRelativeTo), list, false, enumString);
				return false;
			}
		}

		[HarmonyPatch(typeof(PropertyControl_Toggle), "useButtons", MethodType.Getter)]
		public static class ForceDropdown
		{
			public static void Postfix(PropertyControl_Toggle __instance, ref bool __result)
			{
				if (NeoEditor.Instance == null) return;
				if (__instance.enumValList.Count >= 3 || __instance.propertyInfo.stringDropdown)
				{
					__result = false;
					return;
				}

				__result = true;
			}
		}

		[HarmonyPatch(typeof(PropertiesPanel), "Init")]
		public static class FloorNumProperty
		{
			public static void Prefix(LevelEventInfo levelEventInfo)
			{
				if (NeoEditor.Instance == null) return;
				if (
					!GCS.settingsInfo.Values.Contains(levelEventInfo)
					&& !levelEventInfo.propertiesInfo.Keys.Contains("floor")
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
					levelEventInfo.propertiesInfo = new Dictionary<string, ADOFAI.PropertyInfo>
					{
						{ "floor", propertyInfo }
					}.Concat(levelEventInfo.propertiesInfo).ToDictionary(k => k.Key, v => v.Value);
				}
			}
		}

		[HarmonyPatch(typeof(PropertyControl_Text), "Validate")]
		public static class SyncTimeline
		{
			public static void Postfix(PropertyControl_Text __instance, string __result)
			{
				NeoEditor editor = NeoEditor.Instance;
				if (editor == null) return;
				if (__instance.propertyInfo.name.Contains("angleOffset"))
				{
					float value = 1f;
					float.TryParse(__result, out value);
					editor.timelinePanel.UpdateSelectedEventPos(value);
				}
				else if (__instance.propertyInfo.name == "floor")
				{
					int value2 = 1;
					float f;
					if (float.TryParse(__result, out f))
					{
						value2 = Mathf.RoundToInt(f);
					}
					editor.timelinePanel.UpdateSelectedEventPos(value2);
				}
			}
		}

		[HarmonyPatch(typeof(PropertiesPanel), "SetProperties")]
		public static class UpdateLayoutAtSetProperties
		{
			public static void Postfix(PropertiesPanel __instance)
			{
				if (NeoEditor.Instance == null) return;
				__instance.StartCoroutine(InvokeAtNextFrame(() => LayoutRebuilder.ForceRebuildLayoutImmediate(__instance.content)));
			}
		}

		[HarmonyPatch(typeof(PropertiesPanel), "RenderControl")]
		public static class UpdateLayoutAtOnOff
		{
			public static void Postfix(PropertiesPanel __instance)
			{
				if (NeoEditor.Instance == null) return;
				foreach (var property in __instance.properties.Values)
				{
					property.enabledButton.onClick.AddListener(() => __instance.StartCoroutine(InvokeAtNextFrame(() => LayoutRebuilder.ForceRebuildLayoutImmediate(__instance.content))));
				}
			}
		}

		[HarmonyPatch(typeof(PropertiesSubTabButton), "SetSelected")]
		public static class TabButtonSelectedPatch
		{
			public static void Prefix(PropertiesSubTabButton __instance, bool selected)
			{
				if (NeoEditor.Instance == null) return;
				__instance.button.interactable = !selected;
			}
		}

		//[HarmonyPatch(typeof(InspectorPanel), "ToggleArtistPopup")]
		public static class ArtistPopupPositionPatch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = new List<CodeInstruction>(instructions);

				for (int i = 0; i < codes.Count; i++)
				{
					CodeInstruction code = codes[i];
					if (code.opcode != OpCodes.Ldc_R4)
						continue;

					if (i < codes.Count - 1 && codes[i + 1].opcode == OpCodes.Sub)
					{
						code.operand = 15f;
						break;
					}
				}
				return codes.AsEnumerable();
			}
		}
	}
}
