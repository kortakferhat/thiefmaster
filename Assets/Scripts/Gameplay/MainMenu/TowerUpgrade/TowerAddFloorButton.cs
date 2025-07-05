using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.MainMenu.TowerUpgrade
{
    public class TowerAddFloorButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI floorNameText;
        [SerializeField] private TextMeshProUGUI priceText;
        
        [SerializeField] private Image icon;
        [SerializeField] private Button button;
        
        public void Initialize(string floorName, int price, Sprite icon)
        {
            SetIcon(icon);
            SetFloorName(floorName);
            SetPrice(price);
        }
        
        public void SetPrice(int price)
        {
            priceText.text = "$" + price;
        }
        
        public void SetIcon(Sprite iconSprite)
        {
            icon.sprite = iconSprite;
        }
        
        public void SetFloorName(string floorName)
        {
            floorNameText.text = floorName;
        }
        
        public void SetInteractable(bool isInteractable)
        {
            var button = GetComponent<Button>();
            if (button != null)
            {
                button.interactable = isInteractable;
            }
        }
        
        public void AddOnClickListener(UnityEngine.Events.UnityAction action)
        {
            button.onClick.AddListener(action);
        }
        
        public void RemoveOnClickListener(UnityEngine.Events.UnityAction action)
        {
            button.onClick.RemoveListener(action);
        }
        
        public void RemoveAllOnClickListeners()
        {
            button.onClick.RemoveAllListeners();
        }
    }
}