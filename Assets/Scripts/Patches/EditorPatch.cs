using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoEditor.Patches
{
	public static class EditorPatch
	{
		//[HarmonyPatch(typeof(scnEditor), "Awake")]
		//public static class DisableEditor
		//{
		//	public static void Postfix(scnEditor __instance)
		//	{
		//		if (NeoEditor.Instance == null) return;
		//		__instance.gameObject.SetActive(false);
		//	}
		//}
	}
}
