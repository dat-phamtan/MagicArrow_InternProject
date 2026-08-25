using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundLibrary1", menuName = "Scriptable Objects/SoundLibrary1")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public class SfxEntry
    {
        public SfxId id;
        public AudioClip clip;
        public float volume = 1f;
    }

    [Serializable]
    public class MusicEntry
    {
        public MusicId id;
        public AudioClip clip;
        public float volume = 1f;
    }

    public SfxEntry[] sfxEntries;
    public MusicEntry[] musicEntries;
}
