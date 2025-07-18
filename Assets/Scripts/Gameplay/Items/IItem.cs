using UnityEngine;

namespace Gameplay.Collectables
{
    
    
    public interface IItem
    {
        string ItemName { get; }
        void Collect();
        void MoveTo(Vector3 position = default);
    }
}