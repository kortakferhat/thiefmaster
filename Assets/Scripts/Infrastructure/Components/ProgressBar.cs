using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure
{
    public class ProgressBar : MonoBehaviour
    {
        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                UpdateProgressBar();
            }
        }

        public float CurrentValue
        {
            get => _currentValue;
        }

        [SerializeField] private Image _barImage;
        [SerializeField] private float _currentValue;
        [SerializeField] private float _maxValue;

        private Tween _fillTween;

        public void Initialize(float maxValue, float currentValue)
        {
            _maxValue = maxValue;
            _currentValue = currentValue;

            UpdateProgressBar();
        }
        
        public void SetCurrentValue(float value, Action onComplete = null)
        {
            _currentValue = value;
            UpdateProgressBar(onComplete);
        }

        private void UpdateProgressBar(Action onComplete = null)
        {
            if (MaxValue <= 0)
                return;

            var fillAmountBefore = _barImage.fillAmount;
            var fillAmount = CurrentValue / MaxValue;
            
            _fillTween.Kill(true);
            _fillTween = null;
            
            _fillTween = DOVirtual.Float(fillAmountBefore, fillAmount, 0.25f, (value) =>
            {
                _barImage.fillAmount = value;
            }).SetEase(Ease.OutCirc).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
    }
}