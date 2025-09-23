using Infrastructure;

namespace Infrastructure.Managers.AudioManager
{
    /// <summary>
    /// Static helper class for easy access to AudioManager functionality
    /// </summary>
    public static class AudioManagerHelper
    {
        private static IAudioManager audioManager;
        
        private static IAudioManager AudioManager
        {
            get
            {
                if (audioManager == null)
                    audioManager = ServiceLocator.Get<IAudioManager>();
                return audioManager;
            }
        }
        
        // Quick access methods for SFX
        public static void PlaySFX(string audioKey) => AudioManager?.PlaySFX(audioKey);
        public static void PlaySFX(string audioKey, UnityEngine.Vector3 position) => AudioManager?.PlaySFX(audioKey, position);
        public static void PlaySFX(string audioKey, float volume) => AudioManager?.PlaySFX(audioKey, volume);
        
        // Quick access methods for UI sounds
        public static void PlayUI(string uiSoundKey) => AudioManager?.PlayUI(uiSoundKey);
        
        // Quick access methods for music
        public static void PlayMusic(string musicKey) => AudioManager?.PlayMusic(musicKey);
        public static void StopMusic() => AudioManager?.StopMusic();
        public static void FadeMusic(string musicKey, float duration = 1f) => AudioManager?.FadeMusicIn(musicKey, duration);
        public static void CrossfadeMusic(string newMusicKey, float duration = 1f) => AudioManager?.CrossfadeMusic(newMusicKey, duration);
        
        // Volume controls
        public static void SetMasterVolume(float volume) => AudioManager?.SetMasterVolume(volume);
        public static void SetMusicVolume(float volume) => AudioManager?.SetMusicVolume(volume);
        public static void SetSFXVolume(float volume) => AudioManager?.SetSFXVolume(volume);
        public static void SetUIVolume(float volume) => AudioManager?.SetUIVolume(volume);
        
        // Get volume values
        public static float GetMasterVolume() => AudioManager?.GetMasterVolume() ?? 1f;
        public static float GetMusicVolume() => AudioManager?.GetMusicVolume() ?? 1f;
        public static float GetSFXVolume() => AudioManager?.GetSFXVolume() ?? 1f;
        public static float GetUIVolume() => AudioManager?.GetUIVolume() ?? 1f;
        
        // Utility
        public static void StopAllSFX() => AudioManager?.StopAllSFX();
        public static void StopAllAudio() => AudioManager?.StopAllAudio();
        
        // Common audio keys - define these based on your game's needs
        public static class SFX
        {
            public const string BUTTON_CLICK = "button_click";
            public const string COIN_COLLECT = "coin_collect";
            public const string ENEMY_HIT = "enemy_hit";
            public const string PLAYER_MOVE = "player_move";
            public const string LEVEL_COMPLETE = "level_complete";
            public const string GAME_OVER = "game_over";
        }
        
        public static class Music
        {
            public const string MENU_THEME = "menu_theme";
            public const string GAMEPLAY_THEME = "gameplay_theme";
            public const string VICTORY_THEME = "victory_theme";
        }
        
        public static class UI
        {
            public const string BUTTON_HOVER = "button_hover";
            public const string POPUP_OPEN = "popup_open";
            public const string POPUP_CLOSE = "popup_close";
            public const string TAB_SWITCH = "tab_switch";
        }
    }
}
