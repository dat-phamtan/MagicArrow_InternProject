using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Data
{
    public enum LevelState { UNPLAYED, COMPLETED, NOTCOMLETED };
    public enum Hardness { NORMAL, HARD, SUPERHARD};
    public class LevelData
    {
        public int LevelId { get ; set; }
        public int Star { get; set; }
        public LevelState LevelState { get; set; }
        public Hardness Hardness { get; set; }
        public BoardData BoardData { get; set; }
    }
}
