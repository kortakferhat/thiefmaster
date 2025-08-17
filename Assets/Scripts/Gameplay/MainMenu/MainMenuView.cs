using Gameplay.MVP;
using TMPro;
using UnityEngine;

namespace Gameplay.MainMenu
{
    public class MainMenuView : MonoBehaviour, IView
    {
        //public Button towerPopupButton;

        [SerializeField] private TextMeshProUGUI remainingMovesText;
        
        //public event Action OnTowerPopupButtonClicked;

        private void Awake()
        {
            //towerPopupButton.onClick.AddListener(() => OnTowerPopupButtonClicked?.Invoke());
        }
        
        public void SetRemainingMovesText(int remainingMoves)
        {
            remainingMovesText.text = remainingMoves.ToString();
            
            var textColor = remainingMoves > 3 ? Color.white : Color.yellow;
            remainingMovesText.color = textColor;
            
        }
    }
}