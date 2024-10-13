using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NeoEditor.Menu
{
    public class MenuContent : MonoBehaviour, IPointerExitHandler
    {
        public RectTransform rect;
        public Canvas canvas;
        public List<MenuContent> parents = new List<MenuContent>();
        public MenuItem item;
        public List<MenuButton> childs = new List<MenuButton>();

        public void OnPointerExit(PointerEventData eventData)
        {
            gameObject.SetActive(false);
        }
    }
}
