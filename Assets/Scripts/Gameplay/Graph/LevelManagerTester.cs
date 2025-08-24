using Infrastructure;
using Infrastructure.Managers.LevelManager;
using UnityEngine;
using NaughtyAttributes;

namespace Gameplay.Graph
{
    public class LevelManagerTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [Required] public GraphScriptableObject testLevel1;
        [Required] public GraphScriptableObject testLevel2;
        
        [Header("Current Level Info")]
        [ReadOnly] public int currentLevelIndex = -1;
        [ReadOnly] public string currentLevelName = "None";
        
        private ILevelManager _levelManager;

        public void Initialize()
        {
            _levelManager = ServiceLocator.Get<ILevelManager>();
            
            if (_levelManager != null)
            {
                _levelManager.OnLevelChanged += OnLevelChanged;
                _levelManager.OnLevelLoaded += OnLevelLoaded;
                _levelManager.OnLevelCompleted += OnLevelCompleted;
                _levelManager.OnLevelFailed += OnLevelFailed;
            }
            else
            {
                Debug.LogError("LevelManager not found! Make sure GameBootstrapper has initialized it.");
            }
        }
        
        private void OnDestroy()
        {
            if (_levelManager != null)
            {
                _levelManager.OnLevelChanged -= OnLevelChanged;
                _levelManager.OnLevelLoaded -= OnLevelLoaded;
                _levelManager.OnLevelCompleted -= OnLevelCompleted;
                _levelManager.OnLevelFailed -= OnLevelFailed;
            }
        }
        
        #region Inspector Buttons
        
        [Button("Load Test Level 1")]
        [EnableIf("testLevel1")]
        public void LoadTestLevel1()
        {
            if (testLevel1 != null)
            {
                Debug.Log("Loading Test Level 1...");
                _levelManager?.LoadLevel(testLevel1);
            }
            else
            {
                Debug.LogWarning("Test Level 1 is not assigned!");
            }
        }
        
        [Button("Load Test Level 2")]
        [EnableIf("testLevel2")]
        public void LoadTestLevel2()
        {
            if (testLevel2 != null)
            {
                Debug.Log("Loading Test Level 2...");
                _levelManager?.LoadLevel(testLevel2);
            }
            else
            {
                Debug.LogWarning("Test Level 2 is not assigned!");
            }
        }
        
        [Button("Load Level 1 (Addressable)")]
        public void LoadLevel1()
        {
            Debug.Log("Loading Level 1 from Addressable (key: 'level_0')...");
            _levelManager?.LoadLevel(1);
        }

        [Button("Load Level 2 (Addressable)")]
        public void LoadLevel2()
        {
            Debug.Log("Loading Level 2 from Addressable (key: 'level_1')...");
            _levelManager?.LoadLevel(2);
        }

        [Button("Reset Level Progress")]
        public void ResetLevelProgress()
        {
            Debug.Log("Resetting level progress...");
            _levelManager?.ResetLevelProgress();
        }
        
        [Button("Complete Current Level")]
        public void CompleteCurrentLevel()
        {
            Debug.Log("Completing current level...");
            _levelManager?.CompleteLevel();
        }
        
        [Button("Fail Current Level")]
        public void FailCurrentLevel()
        {
            Debug.Log("Failing current level...");
            _levelManager?.FailLevel();
        }
        
        [Button("Restart Current Level")]
        public void RestartCurrentLevel()
        {
            Debug.Log("Restarting current level...");
            _levelManager?.RestartLevel();
        }
        
        [Button("Next Level")]
        public void NextLevel()
        {
            Debug.Log("Going to next level...");
            _levelManager?.NextLevel();
        }
        
        [Button("Reinitialize LevelManager")]
        public async void ReinitializeLevelManager()
        {
            if (_levelManager != null)
            {
                Debug.Log("Reinitializing LevelManager...");
                await _levelManager.Initialize();
                Debug.Log("LevelManager reinitialized successfully!");
            }
            else
            {
                Debug.LogWarning("LevelManager is not available!");
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnLevelChanged(int levelIndex)
        {
            currentLevelIndex = levelIndex;
            Debug.Log($"[LevelManagerTester] Level changed to: {levelIndex}");
        }
        
        private void OnLevelLoaded(GraphScriptableObject levelGraph)
        {
            currentLevelName = levelGraph != null ? levelGraph.graphName : "Unknown";
            Debug.Log($"[LevelManagerTester] Level loaded: {currentLevelName}");
        }
        
        private void OnLevelCompleted()
        {
            Debug.Log($"[LevelManagerTester] Level {currentLevelIndex} ({currentLevelName}) COMPLETED!");
        }
        
        private void OnLevelFailed()
        {
            Debug.Log($"[LevelManagerTester] Level {currentLevelIndex} ({currentLevelName}) FAILED!");
        }
        
        #endregion
        
        #region Debug Info
        
        [Button("Print Current Level Info")]
        public void PrintCurrentLevelInfo()
        {
            if (_levelManager != null)
            {
                Debug.Log($"Current Level Index: {_levelManager.CurrentLevel}");
                Debug.Log($"Current Level Graph: {(_levelManager.CurrentLevelGraph != null ? _levelManager.CurrentLevelGraph.graphName : "None")}");
            }
        }
        
        [Button("Test All Functions")]
        public void TestAllFunctions()
        {
            Debug.Log("=== Testing All LevelManager Functions ===");
            PrintCurrentLevelInfo();
            
            if (testLevel1 != null)
            {
                LoadTestLevel1();
                Invoke(nameof(CompleteCurrentLevel), 2f);
            }
        }
        
        #endregion
    }
} 