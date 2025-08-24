using System.Threading.Tasks;
using UnityEngine;

namespace Infrastructure.Managers
{
    public interface IViewManager : IService
    {
        Task<T> ShowViewAsync<T>(string address) where T : IScreenView;
        void HideView(IScreenView screenView);
        Task<T> ShowPopupAsync<T>(string address, object data = null) where T : IPopup;
        void HidePopup(IPopup popup);
    }
}