using System;
using Gameplay.Animations;
using Gameplay.Floors;
using Gameplay.MVP;
using Infrastructure;
using TMPro;
using TowerClicker.Infrastructure.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.MVP.Tower
{
    public class TowerView : MonoBehaviour, IView
    {
        public event Action OnUpgradeButtonClicked;

        [SerializeField] private Transform visualRoot;
        [SerializeField] private ProgressBar experienceBar;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button upgradeButton;
        
        private TowerHitAnimator _towerHitAnimator;

        public void Initialize(ITowerFloorManager towerFloorManager)
        {
            _towerHitAnimator = new TowerHitAnimator(visualRoot, towerFloorManager);
            upgradeButton.onClick.AddListener(() => OnUpgradeButtonClicked?.Invoke());
        }

        public void UpdateExperienceBar(int maxValue, int currentValue)
        {
            experienceBar.Initialize(maxValue, currentValue);
        }

        public void SetExperienceValue(int currentValue, Action onComplete = null)
        {
            experienceBar.SetCurrentValue(currentValue, onComplete);
        }

        public void UpdateLevelText(int level)
        {
            levelText.text = $"LV: {level}";
        }

        public void PlayHitAnimation()
        {
            _towerHitAnimator.PlayHitAnimation();
        }
        
        private void OnDestroy()
        {
            upgradeButton?.onClick.RemoveAllListeners();
        }
    }
}