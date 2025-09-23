using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Infrastructure.Managers.AudioManager
{
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;
        
        private AudioConfigurationSO audioConfig;
        private Transform audioSourceParent;
        
        // Audio source pools
        private Queue<AudioSource> sfxAudioSourcePool;
        private Queue<AudioSource> uiAudioSourcePool;
        private List<AudioSource> activeSFXSources;
        private List<AudioSource> activeUISources;
        
        // Music management
        private AudioSource musicAudioSource;
        private AudioSource fadingMusicAudioSource; // For crossfading
        private string currentMusicKey;
        private CancellationTokenSource musicFadeCancellationTokenSource;
        
        // Volume settings
        private float masterVolume = 1f;
        private float musicVolume = 0.7f;
        private float sfxVolume = 0.8f;
        private float uiVolume = 0.9f;
        
        // Constants
        private const string AUDIO_CONFIG_ADDRESS = "AudioConfiguration";
        private const string MASTER_VOLUME_KEY = "AudioManager_MasterVolume";
        private const string MUSIC_VOLUME_KEY = "AudioManager_MusicVolume";
        private const string SFX_VOLUME_KEY = "AudioManager_SFXVolume";
        private const string UI_VOLUME_KEY = "AudioManager_UIVolume";
        
        // PUBLIC METHODS
        
        public async void Initialize()
        {
            if (enableDebugLogs) Debug.Log("[AudioManager] Starting initialization...");
            
            // Load audio configuration
            audioConfig = await Addressables.LoadAssetAsync<AudioConfigurationSO>(AUDIO_CONFIG_ADDRESS)
                .ToUniTask(cancellationToken: destroyCancellationToken);
            
            if (audioConfig == null)
            {
                Debug.LogError("[AudioManager] Failed to load AudioConfiguration!");
                return;
            }
            
            // Create parent for audio sources
            audioSourceParent = new GameObject("AudioSources").transform;
            audioSourceParent.SetParent(transform);
            
            // Load saved volume settings
            LoadVolumeSettings();
            
            // Initialize pools
            InitializeAudioSourcePools();
            
            // Initialize music audio source
            InitializeMusicAudioSource();
            
            if (enableDebugLogs) 
                Debug.Log($"[AudioManager] Initialization complete. Pool size: {audioConfig.audioSourcePoolSize}");
        }
        
        // SFX Methods
        public void PlaySFX(string audioKey)
        {
            PlaySFXInternal(audioKey, Vector3.zero, 1f, false);
        }
        
        public void PlaySFX(string audioKey, Vector3 position)
        {
            PlaySFXInternal(audioKey, position, 1f, true);
        }
        
        public void PlaySFX(string audioKey, float volume)
        {
            PlaySFXInternal(audioKey, Vector3.zero, volume, false);
        }
        
        public void PlaySFX(string audioKey, Vector3 position, float volume)
        {
            PlaySFXInternal(audioKey, position, volume, true);
        }
        
        // UI Methods
        public void PlayUI(string uiSoundKey)
        {
            var clipData = audioConfig.GetUIClip(uiSoundKey);
            if (clipData?.audioClip == null)
            {
                if (enableDebugLogs) Debug.LogWarning($"[AudioManager] UI clip not found: {uiSoundKey}");
                return;
            }
            
            var audioSource = GetUIAudioSource();
            if (audioSource == null)
            {
                if (enableDebugLogs) Debug.LogWarning("[AudioManager] No available UI audio sources");
                return;
            }
            
            ConfigureAudioSource(audioSource, clipData, uiVolume * masterVolume, false);
            
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
            
            ReturnUIAudioSourceAfterDelayAsync(audioSource, clipData.audioClip.length / audioSource.pitch).Forget();
        }
        
        // Music Methods
        public void PlayMusic(string musicKey)
        {
            if (currentMusicKey == musicKey && musicAudioSource.isPlaying)
            {
                return; // Already playing this music
            }
            
            var clipData = audioConfig.GetMusicClip(musicKey);
            if (clipData?.audioClip == null)
            {
                if (enableDebugLogs) Debug.LogWarning($"[AudioManager] Music clip not found: {musicKey}");
                return;
            }
            
            StopMusicFade();
            
            musicAudioSource.clip = clipData.audioClip;
            musicAudioSource.volume = clipData.GetRandomizedVolume() * musicVolume * masterVolume;
            musicAudioSource.pitch = clipData.GetRandomizedPitch();
            musicAudioSource.loop = clipData.loop;
            musicAudioSource.Play();
            
            currentMusicKey = musicKey;
        }
        
        public void StopMusic()
        {
            StopMusicFade();
            musicAudioSource.Stop();
            currentMusicKey = null;
        }
        
        public void PauseMusic()
        {
            musicAudioSource.Pause();
        }
        
        public void ResumeMusic()
        {
            musicAudioSource.UnPause();
        }
        
        public void FadeMusicIn(string musicKey, float duration = 1f)
        {
            if (string.IsNullOrEmpty(musicKey)) return;
            
            var clipData = audioConfig.GetMusicClip(musicKey);
            if (clipData?.audioClip == null) return;
            
            StopMusicFade();
            
            musicAudioSource.clip = clipData.audioClip;
            musicAudioSource.volume = 0f;
            musicAudioSource.pitch = clipData.GetRandomizedPitch();
            musicAudioSource.loop = clipData.loop;
            musicAudioSource.Play();
            
            float targetVolume = clipData.GetRandomizedVolume() * musicVolume * masterVolume;
            FadeAudioSourceAsync(musicAudioSource, 0f, targetVolume, duration, audioConfig.fadeInCurve).Forget();
            
            currentMusicKey = musicKey;
        }
        
        public void FadeMusicOut(float duration = 1f)
        {
            if (!musicAudioSource.isPlaying) return;
            
            StopMusicFade();
            
            float currentVolume = musicAudioSource.volume;
            FadeAudioSourceAndStopAsync(musicAudioSource, currentVolume, 0f, duration, audioConfig.fadeOutCurve).Forget();
        }
        
        public void CrossfadeMusic(string newMusicKey, float duration = 1f)
        {
            if (string.IsNullOrEmpty(newMusicKey)) return;
            
            var clipData = audioConfig.GetMusicClip(newMusicKey);
            if (clipData?.audioClip == null) return;
            
            StopMusicFade();
            
            // Swap audio sources for crossfade
            (musicAudioSource, fadingMusicAudioSource) = (fadingMusicAudioSource, musicAudioSource);
            
            // Configure new music
            musicAudioSource.clip = clipData.audioClip;
            musicAudioSource.volume = 0f;
            musicAudioSource.pitch = clipData.GetRandomizedPitch();
            musicAudioSource.loop = clipData.loop;
            musicAudioSource.Play();
            
            // Start crossfade
            float targetVolume = clipData.GetRandomizedVolume() * musicVolume * masterVolume;
            float currentFadingVolume = fadingMusicAudioSource.volume;
            
            CrossfadeAsync(
                musicAudioSource, 0f, targetVolume,
                fadingMusicAudioSource, currentFadingVolume, 0f,
                duration).Forget();
            
            currentMusicKey = newMusicKey;
        }
        
        // Volume Control Methods
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
            SaveVolumeSettings();
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
            SaveVolumeSettings();
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            SaveVolumeSettings();
        }
        
        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            SaveVolumeSettings();
        }
        
        // Volume Getters
        public float GetMasterVolume() => masterVolume;
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        public float GetUIVolume() => uiVolume;
        
        // Utility Methods
        public void StopAllSFX()
        {
            foreach (var source in activeSFXSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                    ReturnSFXAudioSource(source);
                }
            }
            activeSFXSources.Clear();
            
            foreach (var source in activeUISources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                    ReturnUIAudioSource(source);
                }
            }
            activeUISources.Clear();
        }
        
        public void StopAllAudio()
        {
            StopAllSFX();
            StopMusic();
        }
        
        // PRIVATE METHODS
        
        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, audioConfig.defaultMasterVolume);
            musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, audioConfig.defaultMusicVolume);
            sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, audioConfig.defaultSFXVolume);
            uiVolume = PlayerPrefs.GetFloat(UI_VOLUME_KEY, audioConfig.defaultUIVolume);
        }
        
        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
            PlayerPrefs.SetFloat(UI_VOLUME_KEY, uiVolume);
            PlayerPrefs.Save();
        }
        
        private void InitializeAudioSourcePools()
        {
            sfxAudioSourcePool = new Queue<AudioSource>();
            uiAudioSourcePool = new Queue<AudioSource>();
            activeSFXSources = new List<AudioSource>();
            activeUISources = new List<AudioSource>();
            
            // Create SFX audio sources
            var sfxParent = new GameObject("SFX_AudioSources").transform;
            sfxParent.SetParent(audioSourceParent);
            
            for (int i = 0; i < audioConfig.audioSourcePoolSize; i++)
            {
                var audioSourceGO = new GameObject($"SFX_AudioSource_{i}");
                audioSourceGO.transform.SetParent(sfxParent);
                
                var audioSource = audioSourceGO.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D by default
                
                audioSourceGO.SetActive(false);
                sfxAudioSourcePool.Enqueue(audioSource);
            }
            
            // Create UI audio sources (smaller pool)
            var uiParent = new GameObject("UI_AudioSources").transform;
            uiParent.SetParent(audioSourceParent);
            
            int uiPoolSize = Mathf.Max(3, audioConfig.audioSourcePoolSize / 3);
            for (int i = 0; i < uiPoolSize; i++)
            {
                var audioSourceGO = new GameObject($"UI_AudioSource_{i}");
                audioSourceGO.transform.SetParent(uiParent);
                
                var audioSource = audioSourceGO.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // Always 2D for UI
                
                audioSourceGO.SetActive(false);
                uiAudioSourcePool.Enqueue(audioSource);
            }
        }
        
        private void InitializeMusicAudioSource()
        {
            var musicParent = new GameObject("Music_AudioSources").transform;
            musicParent.SetParent(audioSourceParent);
            
            // Main music source
            var musicGO = new GameObject("Music_AudioSource");
            musicGO.transform.SetParent(musicParent);
            musicAudioSource = musicGO.AddComponent<AudioSource>();
            musicAudioSource.playOnAwake = false;
            musicAudioSource.spatialBlend = 0f; // Always 2D for music
            musicAudioSource.loop = true;
            
            // Fading music source for crossfade
            var fadingMusicGO = new GameObject("FadingMusic_AudioSource");
            fadingMusicGO.transform.SetParent(musicParent);
            fadingMusicAudioSource = fadingMusicGO.AddComponent<AudioSource>();
            fadingMusicAudioSource.playOnAwake = false;
            fadingMusicAudioSource.spatialBlend = 0f;
            fadingMusicAudioSource.loop = true;
            fadingMusicAudioSource.volume = 0f;
        }
        
        private void PlaySFXInternal(string audioKey, Vector3 position, float volumeMultiplier, bool is3D)
        {
            var clipData = audioConfig.GetSFXClip(audioKey);
            if (clipData?.audioClip == null)
            {
                if (enableDebugLogs) Debug.LogWarning($"[AudioManager] SFX clip not found: {audioKey}");
                return;
            }
            
            var audioSource = GetSFXAudioSource();
            if (audioSource == null)
            {
                if (enableDebugLogs) Debug.LogWarning("[AudioManager] No available SFX audio sources");
                return;
            }
            
            ConfigureAudioSource(audioSource, clipData, volumeMultiplier * sfxVolume * masterVolume, is3D);
            
            if (is3D || clipData.is3D)
            {
                audioSource.transform.position = position;
            }
            
            audioSource.gameObject.SetActive(true);
            audioSource.Play();
            
            // Return to pool after clip finishes
            ReturnSFXAudioSourceAfterDelayAsync(audioSource, clipData.audioClip.length / audioSource.pitch).Forget();
        }
        
        private AudioSource GetSFXAudioSource()
        {
            if (sfxAudioSourcePool.Count > 0)
            {
                var audioSource = sfxAudioSourcePool.Dequeue();
                activeSFXSources.Add(audioSource);
                return audioSource;
            }
            
            // If pool is empty, try to find a finished audio source
            for (int i = activeSFXSources.Count - 1; i >= 0; i--)
            {
                if (!activeSFXSources[i].isPlaying)
                {
                    var audioSource = activeSFXSources[i];
                    activeSFXSources.RemoveAt(i);
                    activeSFXSources.Add(audioSource); // Move to end
                    return audioSource;
                }
            }
            
            return null; // All sources are busy
        }
        
        private async UniTaskVoid ReturnSFXAudioSourceAfterDelayAsync(AudioSource audioSource, float delay)
        {
            if (destroyCancellationToken.IsCancellationRequested) return;
            
            await UniTask.WaitForSeconds(delay, cancellationToken: destroyCancellationToken);
            
            if (audioSource != null && !destroyCancellationToken.IsCancellationRequested)
            {
                ReturnSFXAudioSource(audioSource);
            }
        }
        
        private void ReturnSFXAudioSource(AudioSource audioSource)
        {
            activeSFXSources.Remove(audioSource);
            audioSource.gameObject.SetActive(false);
            audioSource.clip = null;
            sfxAudioSourcePool.Enqueue(audioSource);
        }
        
        private AudioSource GetUIAudioSource()
        {
            if (uiAudioSourcePool.Count > 0)
            {
                var audioSource = uiAudioSourcePool.Dequeue();
                activeUISources.Add(audioSource);
                return audioSource;
            }
            
            // Try to find a finished audio source
            for (int i = activeUISources.Count - 1; i >= 0; i--)
            {
                if (!activeUISources[i].isPlaying)
                {
                    var audioSource = activeUISources[i];
                    activeUISources.RemoveAt(i);
                    activeUISources.Add(audioSource);
                    return audioSource;
                }
            }
            
            return null;
        }
        
        private async UniTaskVoid ReturnUIAudioSourceAfterDelayAsync(AudioSource audioSource, float delay)
        {
            if (destroyCancellationToken.IsCancellationRequested) return;
            
            await UniTask.WaitForSeconds(delay, cancellationToken: destroyCancellationToken);
            
            if (audioSource != null && !destroyCancellationToken.IsCancellationRequested)
            {
                ReturnUIAudioSource(audioSource);
            }
        }
        
        private void ReturnUIAudioSource(AudioSource audioSource)
        {
            activeUISources.Remove(audioSource);
            audioSource.gameObject.SetActive(false);
            audioSource.clip = null;
            uiAudioSourcePool.Enqueue(audioSource);
        }
        
        private void StopMusicFade()
        {
            if (musicFadeCancellationTokenSource != null)
            {
                musicFadeCancellationTokenSource.Cancel();
                musicFadeCancellationTokenSource.Dispose();
                musicFadeCancellationTokenSource = null;
            }
        }
        
        private async UniTaskVoid FadeAudioSourceAsync(AudioSource source, float startVolume, float targetVolume, float duration, AnimationCurve curve)
        {
            musicFadeCancellationTokenSource?.Cancel();
            musicFadeCancellationTokenSource?.Dispose();
            musicFadeCancellationTokenSource = new CancellationTokenSource();
            
            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, musicFadeCancellationTokenSource.Token);
            
            float elapsed = 0f;
            
            while (elapsed < duration && !linkedToken.Token.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curveValue = curve.Evaluate(t);
                source.volume = Mathf.Lerp(startVolume, targetVolume, curveValue);
                
                await UniTask.Yield(linkedToken.Token);
            }
            
            if (!linkedToken.Token.IsCancellationRequested)
            {
                source.volume = targetVolume;
            }
        }
        
        private async UniTaskVoid FadeAudioSourceAndStopAsync(AudioSource source, float startVolume, float targetVolume, float duration, AnimationCurve curve)
        {
            musicFadeCancellationTokenSource?.Cancel();
            musicFadeCancellationTokenSource?.Dispose();
            musicFadeCancellationTokenSource = new CancellationTokenSource();
            
            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, musicFadeCancellationTokenSource.Token);
            
            float elapsed = 0f;
            
            while (elapsed < duration && !linkedToken.Token.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curveValue = curve.Evaluate(t);
                source.volume = Mathf.Lerp(startVolume, targetVolume, curveValue);
                
                await UniTask.Yield(linkedToken.Token);
            }
            
            if (!linkedToken.Token.IsCancellationRequested)
            {
                source.volume = targetVolume;
                source.Stop();
                currentMusicKey = null;
            }
        }
        
        private async UniTaskVoid CrossfadeAsync(AudioSource fadeInSource, float fadeInStart, float fadeInTarget,
            AudioSource fadeOutSource, float fadeOutStart, float fadeOutTarget, float duration)
        {
            musicFadeCancellationTokenSource?.Cancel();
            musicFadeCancellationTokenSource?.Dispose();
            musicFadeCancellationTokenSource = new CancellationTokenSource();
            
            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, musicFadeCancellationTokenSource.Token);
            
            float elapsed = 0f;
            
            while (elapsed < duration && !linkedToken.Token.IsCancellationRequested)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                float fadeInCurveValue = audioConfig.fadeInCurve.Evaluate(t);
                float fadeOutCurveValue = audioConfig.fadeOutCurve.Evaluate(t);
                
                fadeInSource.volume = Mathf.Lerp(fadeInStart, fadeInTarget, fadeInCurveValue);
                fadeOutSource.volume = Mathf.Lerp(fadeOutStart, fadeOutTarget, fadeOutCurveValue);
                
                await UniTask.Yield(linkedToken.Token);
            }
            
            if (!linkedToken.Token.IsCancellationRequested)
            {
                fadeInSource.volume = fadeInTarget;
                fadeOutSource.volume = fadeOutTarget;
                fadeOutSource.Stop();
            }
        }
        
        private void UpdateAllVolumes()
        {
            UpdateMusicVolume();
            // SFX and UI volumes are applied when playing, no need to update active sources
        }
        
        private void UpdateMusicVolume()
        {
            if (musicAudioSource != null && musicAudioSource.clip != null)
            {
                var clipData = audioConfig.GetMusicClip(currentMusicKey);
                if (clipData != null)
                {
                    musicAudioSource.volume = clipData.GetRandomizedVolume() * musicVolume * masterVolume;
                }
            }
        }
        
        private void ConfigureAudioSource(AudioSource audioSource, AudioClipData clipData, float finalVolume, bool force3D)
        {
            audioSource.clip = clipData.audioClip;
            audioSource.volume = finalVolume;
            audioSource.pitch = clipData.GetRandomizedPitch();
            audioSource.loop = clipData.loop;
            
            bool use3D = force3D || clipData.is3D;
            audioSource.spatialBlend = use3D ? clipData.spatialBlend : 0f;
            
            if (use3D)
            {
                audioSource.maxDistance = clipData.maxDistance;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
            }
        }
        
        private void OnDestroy()
        {
            StopMusicFade();
            
            if (audioConfig != null)
            {
                Addressables.Release(audioConfig);
            }
        }
    }
}