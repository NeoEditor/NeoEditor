using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace NeoEditor.Patches
{
    public static class PauseMenuPatch
	{
        [HarmonyPatch(typeof(PauseMenu), "UpdateCursorVisibility")]
        public static class CursorVisibilityPatch
        {
            public static void Postfix()
            {
                if (NeoEditor.Instance == null)
                    return;
                Cursor.visible = true;
            }
        }
    }
}
