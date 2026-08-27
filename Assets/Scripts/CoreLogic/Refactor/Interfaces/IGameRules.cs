using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.CoreLogic.Refactor.Interfaces
{
    public interface IGameRules
    {
        public int Heart { get; }
        public int MaxHeart { get; }
        public event Action OnHeartChanged;

        public bool IsFirstFail(int configIndex);
        public void ConsumeFirstFail(int configIndex);
        public void LoseHeart();
        public void RestoreHeart(int amount = 1);
        public bool IsWinConditionMet();
        public bool IsLoseConditionMet();
        public void ResetForNewLevel(int maxHeart = 3);
    }
}
