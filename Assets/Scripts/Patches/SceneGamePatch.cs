using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NeoEditor.Patches
{
	public static class SceneGamePatch
	{
		[HarmonyPatch(typeof(scnGame), "Awake")]
		public static class InternalLevelPatch
		{
			public static void Postfix()
			{
				if (NeoEditor.Instance == null) return;
				GCS.internalLevelName = null;
			}
		}

		//[HarmonyPatch(typeof(scnGame), "Play")]
		public static class PlayWithoutCountdown
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = new List<CodeInstruction>(instructions);
				
				for (int i = 0; i < codes.Count; i++)
				{
					CodeInstruction code = codes[i];
					if (code.opcode != OpCodes.Call) continue;
					
					if (code.operand.GetType().BaseType == typeof(MethodInfo) &&
						(MethodInfo)code.operand == typeof(ADOBase).GetProperty("isLevelEditor", AccessTools.all).GetMethod)
					{
						codes[i + 1].opcode = OpCodes.Brfalse_S;
						break;
					}
				}
				
				return codes.AsEnumerable();
			}
		}

		//[HarmonyPatch(typeof(scnGame), "FinishCustomLevelLoading")]
		public static class FixLoadLevel
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = new List<CodeInstruction>(instructions);
				
				for (int i = 0; i < codes.Count; i++)
				{
					CodeInstruction code = codes[i];
					if (code.opcode != OpCodes.Call) continue;

					if (code.operand.GetType().BaseType == typeof(MethodInfo) &&
						(MethodInfo)code.operand == typeof(ADOBase).GetProperty("isLevelEditor", AccessTools.all).GetMethod)
					{
						if (codes[i + 1].opcode == OpCodes.Brtrue)
							codes[i + 1].opcode = OpCodes.Brfalse;
						else if (codes[i + 1].opcode == OpCodes.Brtrue_S)
							codes[i + 1].opcode = OpCodes.Brfalse_S;
						else if (codes[i + 1].opcode == OpCodes.Brfalse)
							codes[i + 1].opcode = OpCodes.Brtrue;
						else if (codes[i + 1].opcode == OpCodes.Brfalse_S)
							codes[i + 1].opcode = OpCodes.Brtrue_S;
						
						i++;
					}
				}
				return codes.AsEnumerable();
			}
		}
	}
}
