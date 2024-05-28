using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NeoEditor.PopupWindows
{
    public class PopupBase : MonoBehaviour
    {
        public TextMeshProUGUI title;
		public TextMeshProUGUI content;

        public virtual void ClosePopup()
        {
            NeoEditor.Instance.showingPopup = false;
        }
	}
}
