using ADOFAI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Media
{
	public class MediaPanel : MonoBehaviour
	{
		public MediaItem itemPrefab;
		public GameObject content;

		private List<MediaItem> items = new List<MediaItem>();

		private static Dictionary<LevelEventType, List<PropertyInfo>> fileProperties;

		void Start()
		{
			if (fileProperties != null) return;

			fileProperties = new Dictionary<LevelEventType, List<PropertyInfo>>();
			var dictionary = GCS
				.settingsInfo.Concat(GCS.levelEventsInfo)
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			foreach (var eventInfo in dictionary)
			{
				foreach (var property in eventInfo.Value.propertiesInfo)
				{
					if (property.Value.type == PropertyType.File)
					{
						if (!fileProperties.ContainsKey(eventInfo.Value.type))
							fileProperties.Add(eventInfo.Value.type, new List<PropertyInfo>());
						fileProperties[eventInfo.Value.type].Add(property.Value);
					}
				}
			}
		}

		public void SetupItems(NeoEditor editor)
		{
			foreach (var item in items)
				Destroy(item.gameObject);
			items.Clear();

			SortedDictionary<string, MediaType> files = new SortedDictionary<string, MediaType>();

			var list = editor.levelData.levelEvents
				.Concat(editor.levelData.decorations)
				.Concat(editor.levelData.settings);

			foreach (var evt in list)
			{
				if (fileProperties.Keys.Contains(evt.eventType))
				{
					foreach (var property in fileProperties[evt.eventType])
					{
						if (!files.ContainsKey((string)evt[property.name]) &&
							!((string)evt[property.name]).IsNullOrEmpty())
							files.Add((string)evt[property.name], property.fileType switch
							{
								FileType.Audio => MediaType.Audio,
								FileType.Image => MediaType.Image,
								FileType.Video => MediaType.Video,
								_ => MediaType.Level
							});
					}
				}
			}

			foreach (var file in files)
			{
				//Main.Entry.Logger.Log(file.Key);
				var item = Instantiate(itemPrefab, content.transform);
				bool hasImage = editor.customLevel.imgHolder.customSprites.ContainsKey(file.Key);

				if (file.Value == MediaType.Image && !hasImage)
				{
					LoadResult result;
					editor.customLevel.imgHolder
						.AddSprite(file.Key, Path.Combine(Path.GetDirectoryName(ADOBase.levelPath), file.Key), out result);
					if (result == LoadResult.Successful) hasImage = true;
				}

				Sprite sprite = hasImage ?
					editor.customLevel.imgHolder.customSprites[file.Key]
					.GetSprite(scrExtImgHolder.ImageOptions.None) :
					null;

				item.SetMediaItem(file.Value, file.Key, sprite);
				items.Add(item);
			}

			//foreach (var sprite in editor.customLevel.imgHolder.customSprites)
			//{
			//	Main.Entry.Logger.Log(sprite.Key);
			//	var item = Instantiate(itemPrefab, content.transform);
			//	item.SetMediaItem(MediaType.Image, sprite.Key, sprite.Value.GetSprite(scrExtImgHolder.ImageOptions.None));
			//}
			//editor.levelData.decorations
		}
	}
}
