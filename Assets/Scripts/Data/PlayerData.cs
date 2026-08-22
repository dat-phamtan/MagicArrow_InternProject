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
        public int NumMagnifier { get; set; }
        public int NumEraser { get; set; }
        public int NumWand { get; set; }
        public int NumRuler { get; set; }
        public SettingData Setting { get; set; }
        public LevelData[] CurrentLevelsData { get; set; }
    }
}
