using Assets.Scripts.Data;
using Assets.Scripts.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Config
{
    public interface IConfig
    {
        public PlayerData LoadPlayerData();
    }
}
