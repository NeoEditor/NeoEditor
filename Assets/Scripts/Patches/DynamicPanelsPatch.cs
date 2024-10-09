using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using DynamicPanels;
using HarmonyLib;
using UnityEngine;

namespace NeoEditor.Patches
{
	public static class DynamicPanelsPatch
	{
		//[HarmonyPatch(typeof(PanelUtils.Internal), "CreatePanel")]
		public static class LoadPanel
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = new List<CodeInstruction>(instructions);

				for (int i = 0; i < codes.Count; i++)
				{
					CodeInstruction code = codes[i];
					if (code.opcode != OpCodes.Ldstr || codes[i + 1].opcode != OpCodes.Call)
						continue;

					if ((string)code.operand == "DynamicPanel")
					{
						codes[i + 1].opcode = OpCodes.Call;
						codes[i + 1].operand = typeof(LoadPanel).GetMethod("Load", AccessTools.all);
						break;
					}
				}

				return codes.AsEnumerable();
			}

			public static Panel Load(string str)
			{
				return Assets.DynamicPanel;
			}
		}

		[HarmonyPatch(typeof(Panel), "AddTab", new Type[] { typeof(RectTransform), typeof(int) })]
		public static class LoadPanelTab
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = new List<CodeInstruction>(instructions);

				for (int i = 0; i < codes.Count; i++)
				{
					CodeInstruction code = codes[i];
					if (code.opcode != OpCodes.Ldstr)
						continue;

					if ((string)code.operand == "DynamicPanelTab")
					{
						codes[i + 1].opcode = OpCodes.Call;
						codes[i + 1].operand = typeof(LoadPanelTab).GetMethod("Load", AccessTools.all);
						break;
					}
				}

				return codes.AsEnumerable();
			}

			public static PanelTab Load(string str)
			{
				return Assets.DynamicPanelTab;
			}
		}

		[HarmonyPatch(typeof(PanelManager), "InitializePreviewPanel")]
		public static class LoadPanelPreview
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = new List<CodeInstruction>(instructions);

				for (int i = 0; i < codes.Count; i++)
				{
					CodeInstruction code = codes[i];
					if (code.opcode != OpCodes.Ldstr)
						continue;

					if ((string)code.operand == "DynamicPanelPreview")
					{
						codes[i + 1].opcode = OpCodes.Call;
						codes[i + 1].operand = typeof(LoadPanelPreview).GetMethod("Load", AccessTools.all);
						break;
					}
				}

				return codes.AsEnumerable();
			}

			public static RectTransform Load(string str)
			{
				return Assets.DynamicPanelPreview;
			}
		}

		[HarmonyPatch(typeof(DynamicPanelsCanvas), "Start")]
		public static class FixNullException
		{
			public static void Prefix(DynamicPanelsCanvas __instance)
			{
				__instance.Field("initialPanelsAnchored").SetValue(__instance, null);
				__instance.Field("initialPanelsUnanchored").SetValue(__instance, new List<DynamicPanelsCanvas.PanelProperties>());
			}
		}
	}
}
