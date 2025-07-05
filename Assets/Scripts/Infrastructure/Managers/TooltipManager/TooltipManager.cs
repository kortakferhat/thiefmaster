using Infrastructure.Managers.PoolManager;
using TowerClicker.Infrastructure;
using UnityEngine;

namespace Infrastructure.Managers.TooltipManager
{
    public class TooltipManager : MonoBehaviour, ITooltipManager
    {
        private Transform _tooltipsRoot;
        private TooltipView _activeTooltip;
        
        private IPoolManager _poolManager;

        public void Initialize(IPoolManager poolManager, Transform tooltipsRoot)
        {
            _poolManager = poolManager;
            _tooltipsRoot = tooltipsRoot;
        }

        public void ShowTooltip(string message)
        {
            HideActiveTooltip();
            
            var tooltip = _poolManager.Spawn(PoolKeys.Tooltip, _tooltipsRoot).GetComponent<TooltipView>();
            tooltip.transform.localScale = Vector3.one;
            tooltip.transform.localPosition = Vector3.zero;
            tooltip.transform.localRotation = Quaternion.identity;
            
            var rectTransform = tooltip.GetComponent<RectTransform>();
            var offsetMin = rectTransform.offsetMin;
            offsetMin.x = 0;
            rectTransform.offsetMin = offsetMin;
            
            var offsetMax = rectTransform.offsetMax;
            offsetMax.x = 0;
            rectTransform.offsetMax = offsetMax;
            
            tooltip.Show(message, HideActiveTooltip);
            
            tooltip.transform.SetParent(_tooltipsRoot, false);
            _activeTooltip = tooltip;
        }

        public void HideActiveTooltip()
        {
            if (!_activeTooltip) return;
            
            _poolManager.Despawn(PoolKeys.Tooltip, _activeTooltip.gameObject);
            _activeTooltip = null;
        }
    }
}