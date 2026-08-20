using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Data
{
    public enum Language { VN, EN };
    public class SettingData
    {
        public Language Language { get; set; }
        public bool IsMuteMusic { get; set; }
        public bool IsMuteSoundEffect { get; set; }
        public bool IsVibrate { get; set; }
        public bool IsDarkMode { get; set; }
    }
}
