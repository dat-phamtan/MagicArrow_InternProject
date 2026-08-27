using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.CoreLogic.Refactor.Interfaces
{
    public interface IPlayerProgress
    {
        public PlayerData Data { get; }
        public int CurrentLevelIndex { get; set; }

        public void Load(PlayerData data);
        public void SaveWinResult(int heartRemaining);
    }
}
