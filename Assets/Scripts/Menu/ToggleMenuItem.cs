using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADOFAI.Editor;
using UnityEngine;

namespace NeoEditor.Menu
{
    public class ToggleMenuItem : MenuItem
    {
        public Action<bool> action;

        public ToggleMenuItem(
            string text,
            EditorKeybind shortcut,
            Action<bool> action,
            Func<GameObject, bool> onActive = null
        )
            : base(text, shortcut, onActive)
        {
            this.action = action;
        }
    }
}
