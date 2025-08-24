using Cysharp.Threading.Tasks;
using Gameplay.MVP;
using Infrastructure;
using TMPro;
using UnityEngine;

namespace Gameplay.MainMenu
{
    public class MainMenuView : MonoBehaviour, IView
    {
        [SerializeField] private TextMeshProUGUI remainingMovesText;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TextMeshProUGUI pauseText;
        
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

        public void ShowPauseText()
        {
            pauseText.gameObject.SetActive(true);
        }
        
        public void HidePauseText()
        {
            pauseText.gameObject.SetActive(false);
        }

        private void ShowGameOverText()
        {
            gameOverText.gameObject.SetActive(true);
        }

        public void HideGameOverText()
        {
            gameOverText.gameObject.SetActive(false);
        }

        public void HideAllTexts()
        {
            HidePauseText();
            HideGameOverText();
        }

        public void PrepareGameStateChange(GameState argsCurrentState)
        {
            HideAllTexts();
            
            if (argsCurrentState == GameState.Game)
            {
                return;
            }
            
            if (argsCurrentState == GameState.Pause)
            {
                ShowPauseText();
                return;
            }

            if (argsCurrentState == GameState.Finish)
            {
                ShowGameOverText();
                return;
            }
        }
    }
}