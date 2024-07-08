using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADOFAI.Editor;
using UnityEngine;
using UnityEngine.Events;

namespace NeoEditor.Menu
{
    public class MenuItem
    {
        public string text;
        public MenuItem parent;
        public List<MenuItem> subMenus;
        public EditorKeybind shortcut;
        public string shortcutText;
        public Func<GameObject, bool> onActive;

        public MenuItem(string text, Func<GameObject, bool> onActive = null)
        {
            this.text = text;
            subMenus = new List<MenuItem>();
            this.onActive = onActive;
        }

        public MenuItem(string text, EditorKeybind shortcut, Func<GameObject, bool> onActive = null)
            : this(text, onActive)
        {
            this.shortcut = shortcut;
            
            var keyMask = shortcut.modifierMask;
            shortcutText = RDEditorUtils.KeyComboToString(
                keyMask.HasFlag(KeyModifier.Control),
                keyMask.HasFlag(KeyModifier.Shift),
                keyMask.HasFlag(KeyModifier.Alt),
                shortcut.key
            );
        }

        public MenuItem(string text, string shortcut, Func<GameObject, bool> onActive = null)
            : this(text, onActive)
        {
            shortcutText = shortcut;
        }

        public MenuItem AddSubMenu(MenuItem item)
        {
            item.parent = this;
            subMenus.Add(item);
            return item;
        }
    }
}
