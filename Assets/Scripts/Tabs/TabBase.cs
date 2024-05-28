using System.Collections;
using System.Collections.Generic;
using ADOFAI;
using UnityEngine;

namespace NeoEditor.Tabs
{
    public class TabBase : MonoBehaviour
    {
        public virtual void InitTab(Dictionary<string, LevelEventInfo> levelEventsInfo) { }
    }
}
