using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.IO;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.HeartManager
{
    public class HeartManager : IHeartManager
    {
        private const int MaxHeart = 5;
        private static readonly TimeSpan RegenInterval = TimeSpan.FromMinutes(30);

        private readonly IController _controller;
        private readonly IStorage _storage;

        public event Action OnHeartRestored;

        public HeartManager(IController controller, IStorage storage)
        {
            _controller = controller;
            _storage = storage;
        }

        public void Init()
        {
            var data = _controller.GetPlayerData();
            if (data == null)
                return;

            if (data.Heart >= MaxHeart)
            {
                data.NextHeartRegenTime = default;
                return;
            }

            if (data.NextHeartRegenTime == default)
                data.NextHeartRegenTime = DateTime.UtcNow + RegenInterval;

            CatchUpOfflineRegen(data);
        }

        public void Tick()
        {
            var data = _controller.GetPlayerData();
            if (data == null || data.Heart >= MaxHeart)
                return;

            if (data.NextHeartRegenTime == default)
            {
                data.NextHeartRegenTime = DateTime.UtcNow + RegenInterval;
                _storage.Save("PlayerData", data);
                return;
            }

            if (DateTime.UtcNow < data.NextHeartRegenTime)
                return;

            RestoreOneHeart(data);
            _storage.Save("PlayerData", data);
        }

        public TimeSpan GetTimeUntilNextHeart()
        {
            var data = _controller.GetPlayerData();
            if (data == null || data.Heart >= MaxHeart || data.NextHeartRegenTime == default)
                return TimeSpan.Zero;

            var remaining = data.NextHeartRegenTime - DateTime.UtcNow;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        private void CatchUpOfflineRegen(PlayerData data)
        {
            bool changed = false;
            while (data.Heart < MaxHeart && DateTime.UtcNow >= data.NextHeartRegenTime)
            {
                RestoreOneHeart(data);
                changed = true;
            }

            if (changed)
                _storage.Save("PlayerData", data);
        }

        private void RestoreOneHeart(PlayerData data)
        {
            data.Heart = Mathf.Min(data.Heart + 1, MaxHeart);
            data.NextHeartRegenTime = data.Heart < MaxHeart
                ? data.NextHeartRegenTime + RegenInterval 
                : default;

            OnHeartRestored?.Invoke();
        }
    }
}
