using System;
using Gameplay.MVP;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.MainMenu
{
    public class MainMenuView : MonoBehaviour, IView
    {
        //public Button towerPopupButton;

        [SerializeField] private TextMeshProUGUI moneyText;
        
        //public event Action OnTowerPopupButtonClicked;

        private void Awake()
        {
            //towerPopupButton.onClick.AddListener(() => OnTowerPopupButtonClicked?.Invoke());
        }
        
        public void SetMoneyText(int money)
        {
            moneyText.text = $"{money}$";
        }
    }
}