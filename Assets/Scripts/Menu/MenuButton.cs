using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeoEditor.Menu
{
    public class MenuButton : MonoBehaviour
    {
        public RectTransform rect;

        public Button button;
        public TextMeshProUGUI text;
        public TextMeshProUGUI shortcut;
        public Image checkbox;

        public Sprite check;
        public Sprite uncheck;

        public MenuItem info;
        private bool _isChecked;
        public bool isChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                checkbox.sprite = value ? check : uncheck;
                _isChecked = value;
            }
        }

        public void SetEnabled(bool enabled)
        {
            button.interactable = enabled;
            text.color.WithAlpha(enabled ? 1f : 0.5f);
			shortcut.color.WithAlpha(enabled ? 1f : 0.5f);
		}
    }
}
