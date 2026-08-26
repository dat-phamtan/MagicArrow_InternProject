using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.HeartManager
{
    public interface IHeartManager
    {
        public event Action OnHeartRestored;
        public void Init();
        public void Tick();
        public TimeSpan GetTimeUntilNextHeart();
    }
}
