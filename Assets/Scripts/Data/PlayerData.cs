using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Data
{
    public class PlayerData
    {
        public int Gold {  get; set; }
        public int Heart { get; set; }
        public int RegenHour { get; set; }
        public int RegenMinute { get; set; }
        public int Star { get; set; }
        public int CurrentLevelId { get; set; }
        public LevelData[] CurrentLevelsData { get; set; }
    }
}
