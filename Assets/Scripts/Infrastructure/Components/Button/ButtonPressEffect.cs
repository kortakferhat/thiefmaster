namespace Infrastructure.Button
{
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class UIButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private Transform rootTransform;
        [SerializeField] private Vector3 pressedScale = new Vector3(0.9f, 0.9f, 1f);

        private Vector3 _originalScale;

        private void Awake()
        {
            if (rootTransform == null)
                rootTransform = transform;

            _originalScale = rootTransform.localScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            rootTransform.localScale = pressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            rootTransform.localScale = _originalScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            rootTransform.localScale = _originalScale;
        }
    }

}