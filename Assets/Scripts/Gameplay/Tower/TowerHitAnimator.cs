using System.Collections.Generic;
using DG.Tweening;
using Gameplay.Floors;
using TowerClicker.Infrastructure.Managers;
using UnityEngine;

namespace Gameplay.Animations
{
    public class TowerHitAnimator
    {
        // Sabitler
        private const float ShakeRandomMin = -1f;
        private const float ShakeRandomMax = 1f;
        private const float ShakeMagnitude = 0.5f;
        private const float ColorChangeDuration = 0.1f;
        private const float RotationRandomMin = -2f;
        private const float RotationRandomMax = 2f;
        private const float ReturnDuration = 0.2f;
        
        // Özellikler
        private readonly Color _hitColor = new Color(0.3f, .8f, .8f);
        private bool _isHitAnimationPlaying;
        private Sequence _hitAnimationSequence;
        
        // Referanslar
        private readonly Transform _visualRoot;
        private readonly ITowerFloorManager _towerFloorManager;
        
        public TowerHitAnimator(Transform visualRoot, ITowerFloorManager towerFloorManager)
        {
            _visualRoot = visualRoot;
            _towerFloorManager = towerFloorManager;
        }
        
        public void PlayHitAnimation()
        {
            if (_isHitAnimationPlaying)
            {
                _hitAnimationSequence?.Kill();
            }

            _isHitAnimationPlaying = true;

            // Orijinal değerleri kaydet
            Vector3 originalPosition = _visualRoot.localPosition;
            Quaternion originalRotation = _visualRoot.localRotation;

            // Yeni bir sequence oluştur
            _hitAnimationSequence = DOTween.Sequence();

            // Rastgele sallanma yönü
            float xShake = Random.Range(ShakeRandomMin, ShakeRandomMax) * ShakeMagnitude;
            float zShake = Random.Range(ShakeRandomMin, ShakeRandomMax) * ShakeMagnitude;

            // Tüm katları al
            List<TowerFloor> floors = _towerFloorManager.GetFloors();
            Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();

            // Her katın renderer'ını ve orijinal rengini sakla
            foreach (TowerFloor floor in floors)
            {
                Renderer[] renderers = floor.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    if (renderer.material.HasProperty("_Color"))
                    {
                        originalColors[renderer] = renderer.material.color;
                    }
                }
            }

            // Katların rengini vuruş rengine değiştir
            foreach (KeyValuePair<Renderer, Color> pair in originalColors)
            {
                _hitAnimationSequence.Join(pair.Key.material.DOColor(_hitColor, ColorChangeDuration));
            }

            // Pozisyon animasyonları
            _hitAnimationSequence.Join(_visualRoot.DOLocalMoveX(originalPosition.x + xShake, ColorChangeDuration).SetEase(Ease.OutQuad));
            _hitAnimationSequence.Join(_visualRoot.DOLocalMoveZ(originalPosition.z + zShake, ColorChangeDuration).SetEase(Ease.OutQuad));

            // Rotasyon animasyonları
            _hitAnimationSequence.Join(_visualRoot.DOLocalRotate(new Vector3(Random.Range(RotationRandomMin, RotationRandomMax), 0, Random.Range(RotationRandomMin, RotationRandomMax)), ColorChangeDuration));

            // Katların orijinal rengine geri dönüş
            foreach (KeyValuePair<Renderer, Color> pair in originalColors)
            {
                _hitAnimationSequence.Append(pair.Key.material.DOColor(pair.Value, ReturnDuration));
            }

            // Orijinal pozisyona geri dön
            _hitAnimationSequence.Join(_visualRoot.DOLocalMove(originalPosition, ReturnDuration).SetEase(Ease.OutElastic));
            _hitAnimationSequence.Join(_visualRoot.DOLocalRotateQuaternion(originalRotation, ReturnDuration).SetEase(Ease.OutElastic));

            // Sequence tamamlandığında çalışacak callback
            _hitAnimationSequence.OnComplete(() => {
                _isHitAnimationPlaying = false;
                
                // Tam emin olmak için orijinal değerlere geri dön
                _visualRoot.localPosition = originalPosition;
                _visualRoot.localRotation = originalRotation;
                
                // Tüm renderer'ların orijinal renklerine geri dönüp dönmediğini kontrol et
                foreach (KeyValuePair<Renderer, Color> pair in originalColors)
                {
                    pair.Key.material.color = pair.Value;
                }
            });
        }
    }
}