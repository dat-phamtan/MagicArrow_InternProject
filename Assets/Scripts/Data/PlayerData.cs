using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Data
{
    public class PlayerData
    {
        public int Gold {  get; set; }
        public int Heart { get; set; }
        public int Star { get; set; }
        public LevelData[] CurrentLevelsData { get; set; }
    }
}
