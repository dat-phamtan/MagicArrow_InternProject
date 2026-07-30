using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Assets.Scripts.IO
{
    public class LocalStorage : IStorage
    {
        private readonly string _saveDirectory;
        private readonly object _fileLock = new();

        public LocalStorage()
        {
            _saveDirectory = Application.persistentDataPath;
            Debug.Log($"Path: {_saveDirectory}");
        }

        public T Load<T>(string fileName, T defaultData = default)
        {
            string filePath = _saveDirectory + "/" + fileName; 
            if (!File.Exists(filePath))
                    return defaultData;
            lock( _fileLock )
            {
                string loadedData = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(loadedData);
            }
        }

        public void Save<T>(string fileName, T data)
        {
            string filePath = _saveDirectory + "/" + fileName;
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            lock( _fileLock )
            {
                File.WriteAllText(filePath, jsonData);
            }
        }

        public void Append<T>(string fileName, T data)
        {
            string filePath = _saveDirectory + "/" + fileName;
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            lock ( _fileLock)
            {
                File.AppendAllText(filePath, jsonData);
            }
        }
    }
}
