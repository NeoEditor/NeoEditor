using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NeoEditor.Inspector.Timeline
{
    public struct VerticalLineData
    {
        public int id;
        public float x;
        public GameObject obj;
        public FloorNumberText num;

        public VerticalLineData(int i, float posX, GameObject line, FloorNumberText floorId)
        {
            id = i;
            x = posX;
            obj = line;
            num = floorId;
        }
    }
}
