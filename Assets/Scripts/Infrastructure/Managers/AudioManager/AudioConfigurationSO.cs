using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Managers.AudioManager
{
    [CreateAssetMenu(fileName = "AudioConfiguration", menuName = "Config/Audio Configuration")]
    public class AudioConfigurationSO : ScriptableObject
    {
        [Header("Audio Collections")]
        public List<AudioClipData> musicClips = new List<AudioClipData>();
        public List<AudioClipData> sfxClips = new List<AudioClipData>();
        public List<AudioClipData> uiClips = new List<AudioClipData>();
        
        [Header("Pool Settings")]
        [Range(1, 20)]
        public int audioSourcePoolSize = 10;
        
        [Header("Default Volume Settings")]
        [Range(0f, 1f)]
        public float defaultMasterVolume = 1f;
        [Range(0f, 1f)]
        public float defaultMusicVolume = 0.7f;
        [Range(0f, 1f)]
        public float defaultSFXVolume = 0.8f;
        [Range(0f, 1f)]
        public float defaultUIVolume = 0.9f;
        
        [Header("Fade Settings")]
        public float defaultFadeDuration = 1f;
        public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        // Quick lookup dictionaries - populated at runtime
        [NonSerialized]
        private Dictionary<string, AudioClipData> musicLookup;
        [NonSerialized]
        private Dictionary<string, AudioClipData> sfxLookup;
        [NonSerialized]
        private Dictionary<string, AudioClipData> uiLookup;
        
        private void OnEnable()
        {
            BuildLookupTables();
        }
        
        private void BuildLookupTables()
        {
            musicLookup = new Dictionary<string, AudioClipData>();
            sfxLookup = new Dictionary<string, AudioClipData>();
            uiLookup = new Dictionary<string, AudioClipData>();
            
            foreach (var clip in musicClips)
            {
                if (!string.IsNullOrEmpty(clip.key) && clip.audioClip != null)
                    musicLookup[clip.key] = clip;
            }
            
            foreach (var clip in sfxClips)
            {
                if (!string.IsNullOrEmpty(clip.key) && clip.audioClip != null)
                    sfxLookup[clip.key] = clip;
            }
            
            foreach (var clip in uiClips)
            {
                if (!string.IsNullOrEmpty(clip.key) && clip.audioClip != null)
                    uiLookup[clip.key] = clip;
            }
        }
        
        public AudioClipData GetMusicClip(string key)
        {
            if (musicLookup == null) BuildLookupTables();
            return musicLookup.TryGetValue(key, out var clip) ? clip : null;
        }
        
        public AudioClipData GetSFXClip(string key)
        {
            if (sfxLookup == null) BuildLookupTables();
            return sfxLookup.TryGetValue(key, out var clip) ? clip : null;
        }
        
        public AudioClipData GetUIClip(string key)
        {
            if (uiLookup == null) BuildLookupTables();
            return uiLookup.TryGetValue(key, out var clip) ? clip : null;
        }
    }
    
    [System.Serializable]
    public class AudioClipData
    {
        [Header("Basic Settings")]
        public string key;
        public AudioClip audioClip;
        
        [Header("Volume Settings")]
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0f, 1f)]
        public float volumeVariation = 0f;
        
        [Header("Pitch Settings")]
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        [Range(0f, 1f)]
        public float pitchVariation = 0f;
        
        [Header("3D Audio Settings")]
        public bool is3D = false;
        [Range(0f, 500f)]
        public float maxDistance = 50f;
        [Range(0f, 1f)]
        public float spatialBlend = 1f;
        
        [Header("Loop Settings")]
        public bool loop = false;
        
        public float GetRandomizedVolume()
        {
            if (volumeVariation <= 0f) return volume;
            return volume + UnityEngine.Random.Range(-volumeVariation, volumeVariation);
        }
        
        public float GetRandomizedPitch()
        {
            if (pitchVariation <= 0f) return pitch;
            return pitch + UnityEngine.Random.Range(-pitchVariation, pitchVariation);
        }
    }
}
