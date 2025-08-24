using DG.Tweening;
using Infrastructure;
using Infrastructure.Managers;
using Infrastructure.Managers.CameraManager;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.Popups
{
    public class PopupBase : MonoBehaviour, IPopup
    {
        [SerializeField] private Image background;
        [SerializeField] private Button closeButton;
        [SerializeField] private Transform closeButtonRoot;
        [SerializeField] private Canvas canvas;
        
        private Tween backgroundTween;
        private Tween closeButtonTween;
        
        private ICameraManager _cameraManager;
        private IViewManager _viewManager;

        protected virtual void Start()
        {
            _cameraManager = ServiceLocator.Get<ICameraManager>();
            _viewManager = ServiceLocator.Get<IViewManager>();
            
            closeButton.onClick.AddListener(Hide);
        }

        public virtual void Show()
        {
            canvas.worldCamera = _cameraManager.GetTopCamera();
            
            gameObject.SetActive(true);
            
            backgroundTween.Kill(true);
            backgroundTween = null;

            var originalAlpha = background.color.a;
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0);
            backgroundTween = background.DOFade(originalAlpha, 0.5f).SetEase(Ease.OutCirc, 3);
            
            closeButtonTween.Kill(true);
            closeButtonTween = null;
            closeButtonRoot.transform.localScale = Vector3.one * 0.2f;
            closeButtonTween = closeButtonRoot.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack, 3);
        }

        public virtual void Hide()
        {
            _viewManager.HidePopup(this);
        }

        public virtual void DestroyView()
        {
            Destroy(gameObject);
        }

        public virtual void SetData(object data)
        {
            
        }
    }
}