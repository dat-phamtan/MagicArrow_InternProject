using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Assets.Scripts.Data
{
    public class Arrow
    {
        public int XArrowHead {  get; set; }
        public int YArrowHead { get; set; }
        public int[] ArrowIndices { get; set; }

        public Arrow(int xArrowHead, int yArrowHead, int[] arrowIndices)
        {
            XArrowHead = xArrowHead;
            YArrowHead = yArrowHead;
            ArrowIndices = arrowIndices;
        }

    }
}
