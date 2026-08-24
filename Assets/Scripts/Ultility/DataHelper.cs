using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Ultility
{
    public static class DataHelper
    {
        private const string CONFIG_FILE_NAME = "PlayerData.json";
        public static async UniTask CheckFileExist()
        {
            string path = Path.Combine(Application.persistentDataPath, CONFIG_FILE_NAME);
            if (File.Exists(path))
                return;

            string sourcePath = Path.Combine(Application.streamingAssetsPath, CONFIG_FILE_NAME);
        #if UNITY_ANDROID && !UNITY_EDITOR
            using var request = UnityWebRequest.Get(sourcePath);
            await request.SendWebRequest();
            File.WriteAllBytes(path, request.downloadHandler.data);
        #else
            if (File.Exists(sourcePath))
                File.Copy(sourcePath, path);
            await UniTask.CompletedTask;
        #endif
        }
    }
}
