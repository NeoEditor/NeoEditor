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
#if DEBUG
			NeoLogger.Setup(modEntry.Logger, NeoLogger.LogLevel.Debug);
#else
			NeoLogger.Setup(modEntry.Logger, NeoLogger.LogLevel.Info);
#endif

            modEntry.OnToggle = (entry, value) =>
            {
                Enabled = value;

                if (value)
                {
                    harmony.PatchAll(Assembly.GetExecutingAssembly());
                }
                else
                {
                    harmony.UnpatchAll(entry.Info.Id);
                }

                return true;
            };

            modEntry.OnShowGUI = (entry) =>
            {
                modEntry.Info.DisplayName = "NeoEditor (Ctrl + Alt + E)";
            };

			modEntry.OnGUI = (entry) =>
			{
                GUILayout.Label("Ctrl + Alt + E to open NeoEditor.");
			};

			modEntry.OnHideGUI = (entry) =>
            {
                modEntry.Info.DisplayName = "NeoEditor";
            };
        }
    }
}
