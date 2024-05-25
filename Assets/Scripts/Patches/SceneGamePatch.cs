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

		[HarmonyPatch(typeof(scnGame), "Play")]
		public static class PlayWithoutCountdown
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				if(NeoEditor.Instance == null) return instructions;
				var codes = new List<CodeInstruction>(instructions);
				int index = -1;
				for (int i = 0; i < codes.Count; i++)
				{
					CodeInstruction code = codes[i];
					if (code.opcode != OpCodes.Call) continue;
					
					if (code.operand.GetType().BaseType == typeof(MethodInfo) &&
						(MethodInfo)code.operand == typeof(ADOBase).GetProperty("isLevelEditor", AccessTools.all).GetMethod)
					{
						index = i + 1;
					}
				}

				if (index != -1)
				{
					codes[index].opcode = OpCodes.Brfalse_S;
				}
				return codes.AsEnumerable();
			}
		}
	}
}
