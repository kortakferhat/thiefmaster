using Gameplay.MainMenu;
using Gameplay.Views;
using UnityEngine;

namespace Gameplay.Bootstrapper
{
    public class MainMenuBootstrapper : MonoBehaviour, IBootstrapper
    {
        [SerializeField] private MainMenuView mainMenuView;
        
        public void Initialize()
        {
            // Main Menu
            var model = new MainMenuModel();
            var presenter = new MainMenuPresenter(model, mainMenuView);
            presenter.Initialize();
        }
    }
}