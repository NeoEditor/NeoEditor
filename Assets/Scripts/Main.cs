using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using NeoEditor.Patches;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityModManagerNet;

namespace NeoEditor
{
    public static class Main
    {
        public static Harmony harmony;
        public static bool Enabled = false;
        public static UnityModManager.ModEntry Entry;

        public static void Load(UnityModManager.ModEntry modEntry)
        {
            ADOStartup.ModWasAdded(modEntry.Info.Id);
            harmony = new Harmony(modEntry.Info.Id);
            Entry = modEntry;
            Assets.Load();

            modEntry.OnToggle = (entry, value) =>
            {
                Enabled = value;

                if (value)
                {
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                    if (typeof(scrCamera).GetMethod("SetupRTCam", AccessTools.all) != null)
                    {
                        harmony.Patch(
                            typeof(scrCamera).GetMethod("SetupRTCam", AccessTools.all),
                            prefix: typeof(CameraPatch.ForceSetupRTCam).GetMethod(
                                "Prefix",
                                AccessTools.all
                            ),
                            postfix: typeof(CameraPatch.ForceSetupRTCam).GetMethod(
                                "Postfix",
                                AccessTools.all
                            )
                        );
                    }
                }
                else
                {
                    harmony.UnpatchAll(entry.Info.Id);
                }

                return true;
            };

            modEntry.OnGUI = (entry) =>
            {
                if (GUILayout.Button("fjkdslla;"))
                    ADOBase.LoadScene("NeoEditor");
                if (GUILayout.Button("R"))
                {
                    //scnNewEditor.instance.customLevel.RemakePath();
                    //scnNewEditor.instance.customLevel.ResetScene();
                }
            };
        }
    }
}
