using ADOFAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoEditor.Inspector.Timeline
{
    public class LevelEventData
    {
        public readonly float start;
        public readonly float duration;
        public readonly LevelEvent evt;
        public GameObject obj;

        public LevelEventData(float start, float duration, LevelEvent evt)
        {
            this.start = start;
            this.duration = duration;

            this.evt = evt;
        }

        public float end
        {
            get { return start + duration; }
        }

    }
}
