using TowerClicker.Infrastructure;
using UnityEngine;

namespace Infrastructure.Managers.TooltipManager
{
    public interface ITooltipManager : IService
    {
        void Initialize(IPoolManager poolManager, Transform tooltipsRoot);
        void ShowTooltip(string message);
        void HideActiveTooltip();
    }
}