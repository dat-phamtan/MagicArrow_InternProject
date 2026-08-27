using Assets.Scripts.CoreLogic.Refactor.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.CoreLogic.Refactor.Implements
{
    public class GameRulesService : IGameRules
    {
        private int _heart;
        private int _maxHeart = 3;
        private List<bool> _firstFail;

        public int Heart { get { return _heart; } }
        public int MaxHeart { get { return _maxHeart; } }

        public event Action OnHeartChanged;


        public void ConsumeFirstFail(int configIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsFirstFail(int configIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsLoseConditionMet()
        {
            throw new NotImplementedException();
        }

        public bool IsWinConditionMet()
        {
            throw new NotImplementedException();
        }

        public void LoseHeart()
        {
            throw new NotImplementedException();
        }

        public void ResetForNewLevel(int maxHeart = 3)
        {
            throw new NotImplementedException();
        }

        public void RestoreHeart(int amount = 1)
        {
            throw new NotImplementedException();
        }
    }
}
