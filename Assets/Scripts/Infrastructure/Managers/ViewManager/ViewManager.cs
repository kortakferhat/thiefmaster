using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Infrastructure.Managers
{
    public class ViewManager : IViewManager
    {
        private readonly Transform _viewRoot;
        private readonly Transform _popupRoot;
        private readonly List<IScreenView> _activeViews = new();
        private readonly List<IPopup> _activePopups = new();

        public ViewManager(Transform viewRoot, Transform popupRoot)
        {
            _viewRoot = viewRoot;
            _popupRoot = popupRoot;
        }

        public async Task<T> ShowViewAsync<T>(string address) where T : IScreenView
        {
            var handle = Addressables.InstantiateAsync(address, _viewRoot);
            var instance = await handle.Task;
        
            var view = instance.GetComponent<T>();
            if (view == null)
            {
                Debug.LogError($"View of type {typeof(T)} not found on prefab at {address}");
                Object.Destroy(instance);
                return default;
            }

            view.Show();
            _activeViews.Add(view);
            return view;
        }

        public void HideView(IScreenView screenView)
        {
            if (screenView == null) return;
            screenView.Hide();
            screenView.DestroyView();
            _activeViews.Remove(screenView);
        }

        public async Task<T> ShowPopupAsync<T>(string address, object data = null) where T : IPopup
        {
            var handle = Addressables.InstantiateAsync(address, _popupRoot);
            var instance = await handle.Task;
        
            var popup = instance.GetComponent<T>();
            if (popup == null)
            {
                Debug.LogError($"Popup of type {typeof(T)} not found on prefab at {address}");
                Object.Destroy(instance);
                return default;
            }

            if (data != null)
            {
                popup.SetData(data);
            }

            popup.Show();
            _activePopups.Add(popup);
            return popup;
        }

        public void HidePopup(IPopup popup)
        {
            if (popup == null) return;
            popup.DestroyView();
            _activePopups.Remove(popup);
        }
    }
}
