using System.Collections.Generic;
using Gameplay.Floors;
using Gameplay.MainMenu;
using Gameplay.MainMenu.TowerUpgrade;
using Gameplay.TowerDeck;
using Gameplay.Views;
using NUnit.Framework;
using TowerClicker.Infrastructure;
using TowerClicker.Infrastructure.Managers;
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