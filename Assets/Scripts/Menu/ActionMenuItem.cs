using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADOFAI.Editor;
using UnityEngine;

namespace NeoEditor.Menu
{
    public class ActionMenuItem : MenuItem
    {
        public Action action;
        
        public ActionMenuItem(
            string text,
            Action action,
            Func<MenuButton, bool> onActive = null
        )
            : base(text, onActive)
        {
            this.action = action;
        }

        public ActionMenuItem(
            string text,
            EditorKeybind shortcut,
            Action action,
            Func<MenuButton, bool> onActive = null
        )
            : base(text, shortcut, onActive)
        {
            this.action = action;
        }
    }
}
