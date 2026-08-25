using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Sound
{
    public interface ISoundManager
    {
        public bool IsMuteMusic { get; set; }
        public bool IsMuteSoundEffect { get; set; }

        public void Init(IStorage storage, SettingData settings);
        public void BindingEvents(IController controller);
        public void PlaySfx(SfxId id);
        public void PlayMusic(MusicId id, bool fade = true);
        public void StopMusic(bool fade = true);
        public void SetMusicMuted(bool isMuted);
        public void SetSfxMuted(bool isMuted);
    }
}
