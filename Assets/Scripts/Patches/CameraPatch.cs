using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoEditor.Patches
{
	public static class CameraPatch
	{
		//[HarmonyPatch(typeof(scrCamera), "SetupRTCam")]
		public static class ForceSetupRTCam
		{
			public static void Prefix(ref bool enable)
			{
				if (NeoEditor.Instance == null) return;
				enable = true;
			}

			public static void Postfix(scrCamera __instance)
			{
				NeoEditor editor = NeoEditor.Instance;
				if (editor == null) return;
				Assets.GameRenderer = (RenderTexture)__instance.Field("camRT").GetValue(__instance);
				NeoEditor.Instance.uiCamera.targetTexture = (RenderTexture)__instance.Field("camRT").GetValue(__instance);

				foreach(var gameView in editor.gameViews)
					gameView.texture = Assets.GameRenderer;
			}
		}
	}
}
