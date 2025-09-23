using UnityEngine;

namespace Infrastructure.Managers.AudioManager
{
    public interface IAudioManager : IService
    {
        void Initialize();
        
        // Basic audio playback
        void PlaySFX(string audioKey);
        void PlaySFX(string audioKey, Vector3 position);
        void PlaySFX(string audioKey, float volume);
        void PlaySFX(string audioKey, Vector3 position, float volume);
        
        // Background music
        void PlayMusic(string musicKey);
        void StopMusic();
        void PauseMusic();
        void ResumeMusic();
        
        // UI sounds
        void PlayUI(string uiSoundKey);
        
        // Volume controls
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void SetUIVolume(float volume);
        
        float GetMasterVolume();
        float GetMusicVolume();
        float GetSFXVolume();
        float GetUIVolume();
        
        // Audio source management
        void StopAllSFX();
        void StopAllAudio();
        
        // Fade operations
        void FadeMusicIn(string musicKey, float duration = 1f);
        void FadeMusicOut(float duration = 1f);
        void CrossfadeMusic(string newMusicKey, float duration = 1f);
    }
}
