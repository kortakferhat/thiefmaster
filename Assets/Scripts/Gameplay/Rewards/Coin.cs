using System;
using DG.Tweening;
using Gameplay.Events;
using Infrastructure;
using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay.Rewards
{
    public class Coin : BaseEntity
    {
        private IPoolManager poolManager;

        private void Start()
        {
            poolManager = ServiceLocator.Get<IPoolManager>();
        }

        public void MoveToCenter()
        {
            var originalScale = transform.localScale;
            transform.localScale = originalScale * .25f;
            
            transform.DOScale(originalScale, 0.35f).SetEase(Ease.OutBack);
            
            transform.DOMove(Vector3.zero, 0.5f).SetDelay(0.35f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    EventBus.Publish(new PlayerRewardCollectEvent(1));
                    poolManager.DespawnAfterDelay(PoolKeys.Coin, gameObject, 0);
                });
        }
    }
}