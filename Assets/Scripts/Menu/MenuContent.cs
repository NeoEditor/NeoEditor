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
    public class MenuContent : MonoBehaviour
    {
        public RectTransform rect;
        public Canvas canvas;
        public List<MenuContent> parents = new List<MenuContent>();
    }
}
