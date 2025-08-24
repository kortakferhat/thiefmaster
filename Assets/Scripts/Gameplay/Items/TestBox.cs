using DG.Tweening;
using Gameplay.Collectables;
using Gameplay.Events;
using Infrastructure;
using Infrastructure.Managers.PoolManager;
using UnityEngine;

namespace Gameplay.Items
{
    public class TestBox : BaseEntity, IItem
    {
        public string ItemName => "TestBox";
        private IPoolManager poolManager;

        private void Start()
        {
            poolManager = ServiceLocator.Get<IPoolManager>();
        }
        
        public void Collect()
        {
            poolManager.DespawnAfterDelay(PoolKeys.TestBox, gameObject, 0);
        }

        public void MoveTo(Vector3 position = default)
        {
            var originalScale = transform.localScale;
            transform.localScale = originalScale * .25f;
            
            transform.DOScale(originalScale, 0.35f).SetEase(Ease.OutBack);
            
            transform.DOMove(position, 0.5f).SetDelay(0.35f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    EventBus.Publish(new PlayerRewardCollectEvent(1));
                    poolManager.DespawnAfterDelay(PoolKeys.TestBox, gameObject, 0);
                });
        }
    }
}