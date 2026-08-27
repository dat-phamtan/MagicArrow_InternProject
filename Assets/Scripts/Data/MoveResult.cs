using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Data
{
    public class MoveResult
    {
        public bool Success {  get; set; }
        public int ConfigIndex { get; set; }
        public int CollidedConfigIndex { get; set; }
        public int DeltaSteps { get; set; }

        public MoveResult(bool success, int configIndex, int collidedConfigIndex, int deltaSteps)
        {
            Success = success;
            ConfigIndex = configIndex;
            CollidedConfigIndex = collidedConfigIndex;
            DeltaSteps = deltaSteps;
        }
    }
}
