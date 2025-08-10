using UnityEngine;

namespace Gameplay.Character
{
    /// <summary>
    /// Interface for character controller functionality
    /// </summary>
    public interface ICharacterController
    {
        /// <summary>
        /// Reset character to start position
        /// </summary>
        void ResetToStartPosition();
        
        /// <summary>
        /// Get current node ID where character is located
        /// </summary>
        Vector2Int CurrentNodeId { get; }
        
        /// <summary>
        /// Check if character is currently moving
        /// </summary>
        bool IsMoving { get; }
    }
}
