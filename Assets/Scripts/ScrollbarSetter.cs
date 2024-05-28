using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor
{
    public class ScrollbarSetter : MonoBehaviour
    {
        public RectTransform horizontal;
        public RectTransform vertical;

        //      void Start()
        //      {
        //          scroll.onValueChanged.AddListener((Vector2 v) =>
        //          {
        //              StartCoroutine(NextFrame());
        //	});
        //      }

        //IEnumerator NextFrame()
        //      {
        //          yield return null;
        //	horizontal.offsetMax = new Vector2(vertical.gameObject.activeSelf ? -vertical.sizeDelta.x : 0, horizontal.offsetMax.y);
        //	vertical.offsetMin = new Vector2(vertical.offsetMin.x, horizontal.gameObject.activeSelf ? horizontal.sizeDelta.y : 0);
        //}

        void LateUpdate()
        {
            horizontal.offsetMax = new Vector2(
                vertical.gameObject.activeSelf ? -vertical.sizeDelta.x : 0,
                horizontal.offsetMax.y
            );
            vertical.offsetMin = new Vector2(
                vertical.offsetMin.x,
                horizontal.gameObject.activeSelf ? horizontal.sizeDelta.y : 0
            );
        }
    }
}
