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
	public static class ControllerPatch
	{
		//[HarmonyPatch(typeof(scrController), "Update")]
		public static class BlockEscPause
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = new List<CodeInstruction>(instructions);

				for (int i = 0; i < codes.Count; i++)
				{
					CodeInstruction code = codes[i];
					if (code.opcode != OpCodes.Call) continue;

					if (code.operand.GetType().BaseType == typeof(MethodInfo) &&
						(MethodInfo)code.operand == typeof(scrController).GetMethod("TogglePauseGame", AccessTools.all))
					{
						codes[i].opcode = OpCodes.Nop;
						break;
					}
				}
				return codes.AsEnumerable();
			}
		}
	}
}
