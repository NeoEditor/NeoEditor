using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Inspector.Media
{
	public class MediaItem : MonoBehaviour
	{
		public Sprite songIcon;
		public Sprite levelIcon;
		public Sprite imageIcon;
		public Sprite videoIcon;
		public Sprite folderIcon;

		public TextMeshProUGUI label;
		public Image preview;

		public static float size = 100f;

		public void SetMediaItem(MediaType type, string name, Sprite image = null)
		{
			label.text = name;
			if (image != null)
			{
				preview.sprite = image;
			}
			else
			{
				switch (type)
				{
					case MediaType.Audio:
						preview.sprite = songIcon;
						break;
					case MediaType.Image:
						preview.sprite = imageIcon;
						break;
					case MediaType.Video:
						preview.sprite = videoIcon;
						break;
					case MediaType.Level:
						preview.sprite = levelIcon;
						break;
					case MediaType.Folder:
						preview.sprite = folderIcon;
						break;
				}
			}

			Rect rect = preview.sprite.rect;
			if (rect.width < rect.height)
				preview.rectTransform.sizeDelta = new Vector2(rect.width / rect.height * size, size);
			else if (rect.width > rect.height)
				preview.rectTransform.sizeDelta = new Vector2(size, rect.height / rect.width * size);
		}

		void Update()
		{

		}
	}
}
