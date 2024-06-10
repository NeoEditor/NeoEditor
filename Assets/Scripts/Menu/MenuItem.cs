using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace NeoEditor.Menu
{
    public class MenuItem
    {
        public string text;
        public MenuItem parent;
        public List<MenuItem> subMenus;
        public string shortcut;
        public Func<GameObject, bool> onActive;

        public MenuItem(string text, string shortcut, Func<GameObject, bool> onActive = null)
        {
            if (ADOBase.platform == Platform.Mac)
            {
                text.Replace("Ctrl", "Cmd");
                text.Replace("Alt", "Opt");
            }
            this.text = text;
            this.shortcut = shortcut;
            subMenus = new List<MenuItem>();
            this.onActive = onActive;
        }

        public MenuItem AddSubMenu(MenuItem item)
        {
            item.parent = this;
            subMenus.Add(item);
            return item;
        }
    }
}
