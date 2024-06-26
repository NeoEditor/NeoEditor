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

        void Start()
        {

        }

        void LateUpdate()
        {
            Vector2 v = scroll.content.anchoredPosition;
            //if (fixX)
            //    transform.LocalMoveX(transform.localPosition.x - v.x);
            if (fixY)
                transform.LocalMoveY(-v.y);
        }
    }
}
