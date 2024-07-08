using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoEditor.Patches
{
	public static class SceneLevelSelectPatch
	{
		[HarmonyPatch(typeof(scnLevelSelect), "Update")]
		public static class NeoEditorKeyCombo
		{
			public static void Postfix()
			{
				if(RDEditorUtils.CheckForKeyCombo(true, false, KeyCode.E) && RDInput.holdingAlt)
				{
					WipeDirection wipeDirection = WipeDirection.StartsFromRight;
					GCS.sceneToLoad = "NeoEditor";
					scrController.instance.StartLoadingScene(wipeDirection);
				}
			}
		}
	}
}
