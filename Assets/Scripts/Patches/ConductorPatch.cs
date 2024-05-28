using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoEditor.Patches
{
	public static class ConductorPatch
	{
		[HarmonyPatch(typeof(scrConductor), "SetupConductorWithLevelData")]
		public static class PlaybackSpeed
		{
			public static void Postfix(scrConductor __instance)
			{
				if (NeoEditor.Instance == null) return;
				float num = __instance.song.pitch;
				num *= RDInput.holdingControl ? NeoEditor.Instance.playbackSpeed : 1f;
				__instance.song.pitch = num;
			}
		}
	}
}
