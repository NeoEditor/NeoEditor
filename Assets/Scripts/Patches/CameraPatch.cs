﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace NeoEditor.Patches
{
    public static class CameraPatch
    {
        [HarmonyPatch(typeof(scrCamera), "SetupRTCam")]
        public static class ForceSetupRTCam
        {
            public static void Prefix(ref bool enable)
            {
                if (NeoEditor.Instance == null)
                    return;
                enable = true;
            }

            public static void Postfix(scrCamera __instance)
            {
                NeoEditor editor = NeoEditor.Instance;
                if (editor == null)
                    return;
                Assets.GameRenderer = (RenderTexture)__instance.Field("camRT").GetValue(__instance);
                NeoEditor.Instance.uiCamera.targetTexture = Assets.GameRenderer;

                editor.gameView.texture = Assets.GameRenderer;
            }
        }

        [HarmonyPatch(typeof(ffxCustomBackgroundPlus), "StartEffect")]
        public static class ApplyBackgroundColor
        {
            public static void Postfix(ffxCustomBackgroundPlus __instance)
            {
                if (NeoEditor.Instance == null)
                    return;
                NeoEditor.Instance.BGcamstaticCopy.backgroundColor = __instance.color;
            }
        }

        [HarmonyPatch(typeof(scnGame), "SetBackground")]
        public static class ApplyBackgroundColorAtStart
        {
            public static void Postfix(scnGame __instance)
            {
                if (NeoEditor.Instance == null)
                    return;
                NeoEditor.Instance.BGcamstaticCopy.backgroundColor = __instance
                    .levelData
                    .backgroundColor;
            }
        }
    }
}
