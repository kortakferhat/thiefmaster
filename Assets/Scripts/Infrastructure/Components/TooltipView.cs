using System;
using TMPro;
using UnityEngine;
using DG.Tweening; // Add DOTween namespace

namespace Infrastructure
{
    public class TooltipView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI tooltipText;
        
        private readonly float _animationDuration = 0.3f;
        private readonly float _waitDuration = 0.15f;
        private readonly float _moveDistance = 50f;
        
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Sequence _animationSequence;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetText(string message)
        {
            tooltipText.text = message;
        }

        public void Show(string message, Action onComplete = null)
        {
            SetText(message);
            gameObject.SetActive(true);
         
            _animationSequence?.Kill();
            _animationSequence = null;

            // Reset initial state
            _canvasGroup.alpha = 0f;
            var startPosition = _rectTransform.anchoredPosition;
            var bottomPosition = new Vector2(startPosition.x, startPosition.y - _moveDistance);
            _rectTransform.anchoredPosition = bottomPosition;
            
            // Create a new sequence
            _animationSequence = DOTween.Sequence();
            
            // Add fade in and move up animations
            _animationSequence.Append(_canvasGroup.DOFade(1f, _animationDuration).SetEase(Ease.OutQuad));
            _animationSequence.Join(_rectTransform.DOAnchorPos(startPosition, _animationDuration + _waitDuration).SetEase(Ease.InOutSine));
            
            // Add a delay before fading out
            _animationSequence.AppendInterval(_waitDuration);
            _animationSequence.Append(_canvasGroup.DOFade(0f, _animationDuration).SetEase(Ease.InQuad));
            
            // Play the sequence
            _animationSequence.Play();
            
            _animationSequence.OnComplete(() => onComplete?.Invoke());
        }
        
        private void OnDestroy()
        {
            _animationSequence?.Kill();
            _animationSequence = null;
        }

        public void Hide()
        {
            
        }
    }
}