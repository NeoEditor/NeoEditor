using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoEditor.Menu
{
    public class EntryMenuItem : MenuItem
    {
        public EntryMenuItem(string text, Func<MenuButton, bool> onActive = null)
            : base(text, "▶", onActive) { }
    }
}
