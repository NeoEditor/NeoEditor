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
        public bool isChecked = false;
    }
}
