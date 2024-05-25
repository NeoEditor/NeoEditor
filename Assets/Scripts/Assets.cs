using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NeoEditor
{
	public static class Assets
	{
		public static AssetBundle AssetBundle;

		public static void Load()
		{
			AssetBundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(Path.Combine(Main.Entry.Path, "ne_scene")));
		}
	}
}
