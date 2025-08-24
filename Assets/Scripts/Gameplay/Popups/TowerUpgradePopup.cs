using DG.Tweening;
using Infrastructure;
using UnityEngine;

namespace Gameplay.Popups
{
    public class TowerUpgradePopup : PopupBase
    {
        public Transform towerVisual;
        
        private IGameManager _gameManager;

        protected override void Start()
        {
            base.Start();
            
            _gameManager = ServiceLocator.Get<IGameManager>();
        }

        public override void Show()
        {
            base.Show();
            
            _gameManager.PauseGame();

            var originalScale = towerVisual.localScale;
            towerVisual.localScale = originalScale * .2f;
            towerVisual.DOScale(originalScale, 0.5f).SetEase(Ease.OutBack, 3);
            
            towerVisual.rotation = Quaternion.Euler(0, 180, 0);
            towerVisual.DORotate(new Vector3(0, 0, 0), 0.5f).SetEase(Ease.OutBack, 3);
        }

        public override void Hide()
        {
            base.Hide();
            
            _gameManager.StartGame();
        }
    }
}
