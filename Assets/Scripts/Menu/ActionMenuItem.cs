using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoEditor.Menu
{
    public class ActionMenuItem : MenuItem
    {
        public Action action;

        public ActionMenuItem(
            string text,
            string shortcut,
            Action action,
            Func<GameObject, bool> onActive = null
        )
            : base(text, shortcut, onActive)
        {
            this.action = action;
        }
    }
}
