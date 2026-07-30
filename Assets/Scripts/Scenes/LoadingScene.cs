using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.IO;
using UnityEngine;

public class LoadingScene : MonoBehaviour
{
    void Start()
    {
        IStorage storage = new LocalStorage();
        IConfig config = new ConfigManager(storage);
        var manager = new ArrowController(config);

    }

    void Update()
    {
        
    }
}
