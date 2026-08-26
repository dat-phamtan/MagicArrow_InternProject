using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.IO;
using Assets.Scripts.Sound;
using Assets.Scripts.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SfxId { ButtonClick, PopupClick, BoosterClick, ArrowMoveSuccess, ArrowMoveFail, Win, Lose }
public enum MusicId { None, HomeTheme, GamePlayTheme }

public class SoundManager : MonoBehaviour, ISoundManager
{
    public SoundLibrary library;
    public AudioMixerGroup musicMixerGroup;
    public AudioMixerGroup sfxMixerGroup;
    public int sfxPoolSize = 8;
    public float musicFadeDuration = 0.6f;

    private List<AudioSource> _sfxPool = new();
    private AudioSource _musicSourceA;
    private AudioSource _musicSourceB;
    private AudioSource _activeMusicSource;
    private Coroutine _musicFadeRoutine;
    private Dictionary<SfxId, SoundLibrary.SfxEntry> _sfxLookup;
    private Dictionary<MusicId, SoundLibrary.MusicEntry> _musicLookup;

    private IStorage _storage;
    private MusicId _currentMusic = MusicId.None;
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;

    public bool IsMuteMusic { get;  set; }
    public bool IsMuteSoundEffect { get;  set; }


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Locator.Register<ISoundManager>(this);

        BuildDictionary();
        BuildSfxPool();
        BuildMusicSources();
    }

    private void OnDisable()
    {
        SaveSoundSetting();
    }

    //INIT
    public void Init(IStorage storage, SettingData settings)
    {
        _storage = storage;
        if (settings != null)
        {
            IsMuteMusic = settings.IsMuteMusic;
            IsMuteSoundEffect = settings.IsMuteSoundEffect;
        }
        
    }

    public void BindingEvents(IController controller)
    {
        //controller.OnMoveArrowSuccess +=
        //controller.OnMoveArrowFail +=
        //controller.OnLoseHeart +=
        //controller.OnTurnPopupOn +=
    }
    
    private void BuildDictionary()
    {
        _sfxLookup = new Dictionary<SfxId, SoundLibrary.SfxEntry>();
        _musicLookup = new Dictionary<MusicId, SoundLibrary.MusicEntry>();

        foreach (var sfx in library.sfxEntries)
            _sfxLookup[sfx.id] = sfx;
        foreach (var music in library.musicEntries)
            _musicLookup[music.id] = music;
    }

    private void BuildSfxPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
            _sfxPool.Add(audioSource);
        }
    }

    private void BuildMusicSources()
    {
        _musicSourceA = gameObject.AddComponent<AudioSource>();
        _musicSourceB = gameObject.AddComponent<AudioSource>();
        SetUpMusicSource(_musicSourceA);
        SetUpMusicSource(_musicSourceB);
        _activeMusicSource = _musicSourceA;
    }

    private void SetUpMusicSource(AudioSource src)
    {
        src.playOnAwake = false;
        src.loop = true;
        src.outputAudioMixerGroup = musicMixerGroup;
        src.volume = 0f;
    }

    //SFX
    public void PlaySfx(SfxId id)
    {
        if (IsMuteSoundEffect)
            return;
        _sfxLookup.TryGetValue(id, out var entry);
        var src = GetFreeSfxSource();
        src.clip = entry.clip;
        src.volume = entry.volume * _sfxVolume;
        src.pitch = 1f;
        src.Play();
    }
    
    private AudioSource GetFreeSfxSource()
    {
        foreach (var src in _sfxPool)
            if (!src.isPlaying)
                return src;

        return _sfxPool[0];
    }

    //MUSIC
    public void PlayMusic(MusicId id, bool fade = true)
    {
        if (id == _currentMusic)
            return;
        
        _currentMusic = id;
        _musicLookup.TryGetValue(id, out var entry);
        var incomming = _activeMusicSource == _musicSourceA ? _musicSourceB : _musicSourceA;
        incomming.clip = entry.clip;
        incomming.volume = 0f;
        incomming.Play();
        
        if (_musicFadeRoutine != null)
            StopCoroutine(_musicFadeRoutine);

        var fadeDuration = fade ? musicFadeDuration : 0f;
        _musicFadeRoutine = StartCoroutine(CrossfadeMusic(_activeMusicSource, incomming, entry.volume, fadeDuration));
        _activeMusicSource = incomming;
    }

    public void StopMusic(bool fade = true)
    {
        _currentMusic = MusicId.None;
        if (_musicSourceA != null)
            StopCoroutine(_musicFadeRoutine);
        var fadeDuration = fade ? musicFadeDuration: 0f;
        _musicFadeRoutine = StartCoroutine(FadeOutAndStop(_activeMusicSource, fadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioSource outgoing, AudioSource incoming, float entryVolume, float duration)
    {
        var targetVolume = IsMuteMusic ? 0f : entryVolume * _musicVolume;
        
        if (duration <= 0f)
        {
            outgoing.Stop();
            incoming.volume = targetVolume;
            yield break;
        }

        float time = 0f;
        float outgoingVolume = outgoing.volume;
        while (time < duration)
        {
            time += Time.deltaTime;
            float ratio = time / duration;
            outgoing.volume = Mathf.Lerp(outgoingVolume, 0f, ratio);
            incoming.volume = Mathf.Lerp(0f, targetVolume, ratio);
            yield return null;
        }
        outgoing.Stop();
        incoming.volume = targetVolume;
    }

    private IEnumerator FadeOutAndStop(AudioSource src, float duration)
    {
        float startVolume = src.volume;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            var ratio = time / duration;
            src.volume = Mathf.Lerp(startVolume, 0f, ratio);
            yield return null;
        }
        src.Stop();
        src.volume = 0f;
    }

    //SOUND MODIFER
    public void SetMusicMuted(bool isMuted)
    {
        IsMuteMusic = isMuted;
        _activeMusicSource.volume = isMuted ? 0f : 1f;    
    }

    public void SetSfxMuted(bool isMuted)
    {
        IsMuteSoundEffect = isMuted;
        
    }

    //SAVE SOUND SETTING
    private void SaveSoundSetting()
    {
        var playerData = _storage.Load<PlayerData>("PlayerData");
        playerData.Setting.IsMuteMusic = IsMuteMusic;
        playerData.Setting.IsMuteSoundEffect = IsMuteSoundEffect;
        _storage.Save("PlayerData", playerData);
    }

    
}
