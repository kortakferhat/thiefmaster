using Cysharp.Threading.Tasks;
using Gameplay.Events;
using Gameplay.MVP;
using Infrastructure;
using TMPro;
using UnityEngine;

namespace Gameplay.MainMenu
{
    public class MainMenuView : MonoBehaviour, IView
    {
        [SerializeField] private TextMeshProUGUI remainingMovesText;
        [SerializeField] private TextMeshProUGUI winText;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TextMeshProUGUI pauseText;
        
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
        
        private void ShowWinText()
        {
            winText.gameObject.SetActive(true);
        }
        
        private void HideWinText()
        {
            winText.gameObject.SetActive(false);
        }

        public void HideGameOverText()
        {
            gameOverText.gameObject.SetActive(false);
        }

        public void HideAllTexts()
        {
            HidePauseText();
            HideGameOverText();
            HideWinText();
        }

        public void PrepareGameStateChange(GameEvents.GameStateChangeEvent args)
        {
            HideAllTexts();
            
            var currentState = args.CurrentState;
            if (currentState == GameState.Game)
            {
                return;
            }
            
            if (currentState == GameState.Pause)
            {
                ShowPauseText();
                return;
            }

            if (currentState == GameState.Finish)
            {
                var reason = args.Reason;
                if (reason == GameEvents.GameEventChangeReason.Win)
                {
                    ShowWinText();
                }
                else if (reason == GameEvents.GameEventChangeReason.Lose)
                {
                    ShowGameOverText();
                }

                return;
            }
        }
    }
}