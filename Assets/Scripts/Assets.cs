using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NeoEditor
{
	public static class Assets
	{
		public static AssetBundle Scene;
		public static AssetBundle Asset;

		public static RenderTexture GameRenderer;
		public static RenderTexture SceneRenderer;

		public static void Load()
		{
			Scene = AssetBundle.LoadFromMemory(File.ReadAllBytes(Path.Combine(Main.Entry.Path, "ne_scene")));
			Asset = AssetBundle.LoadFromMemory(File.ReadAllBytes(Path.Combine(Main.Entry.Path, "ne_asset")));

			GameRenderer = Asset.LoadAsset<RenderTexture>("GameRenderer");
			SceneRenderer = Asset.LoadAsset<RenderTexture>("SceneRenderer");
		}
	}
}
