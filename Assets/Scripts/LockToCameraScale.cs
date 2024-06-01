using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoEditor
{
    public class LockToCameraScale : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;

        private float spriteX;
        private float spriteY;

        void Start()
        {
            spriteX = spriteRenderer.sprite.bounds.size.x;
            spriteY = spriteRenderer.sprite.bounds.size.y;
            transform.SetParent(scrCamera.instance.camobj.transform, true);
        }

        void Update()
        {
            float screenY = scrCamera.instance.camobj.orthographicSize * 2;
            float screenX = screenY / Screen.height * Screen.width;
            float scale = 4f / 0.03f;
            spriteRenderer.size = new Vector2(
                screenX / spriteX / scale + 0.02f,
                screenY / spriteY / scale + 0.02f
            );
        }
    }
}
