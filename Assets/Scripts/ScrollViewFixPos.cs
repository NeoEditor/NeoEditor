using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor
{
    public class ScrollViewFixPos : MonoBehaviour
    {
        public ScrollRect scroll;

        //not tested.
        //public bool fixX;
        public bool fixY;

        private Vector2 prevScrollPosition;

        void Start()
        {
            prevScrollPosition = scroll.content.anchoredPosition;
        }

        void LateUpdate()
        {
            Vector2 v = scroll.content.anchoredPosition - prevScrollPosition;
            //if (fixX)
            //    transform.LocalMoveX(transform.localPosition.x - v.x);
            if (fixY)
                transform.LocalMoveY(transform.localPosition.y - v.y);
            prevScrollPosition = scroll.content.anchoredPosition;
        }
    }
}
