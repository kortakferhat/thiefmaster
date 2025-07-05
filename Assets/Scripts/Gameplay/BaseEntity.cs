using TowerClicker.Infrastructure;
using UnityEngine;

namespace Gameplay
{
    public abstract class BaseEntity : MonoBehaviour
    {
        protected IGameManager _gameManager;
        
        private bool initialized = false;

        protected virtual void OnEntityUpdate(){}
        protected virtual void OnEntityFixedUpdate(){}
        protected virtual void OnEntityTriggerEnter(Collider other){}
        protected virtual void OnEntityTriggerStay(Collider other){}
        protected virtual void OnEntityTriggerExit(Collider other){}
        
        public virtual void Initialize()
        {
            ResolveManagers();
            initialized = true;
        }

        private void ResolveManagers()
        {
            if (_gameManager == null)
            {
                _gameManager = ServiceLocator.Get<IGameManager>();
            }
        }

        protected virtual void Update()
        {
            if (!initialized) return;
            
            if (_gameManager.State != GameState.Game)   return;
            
            OnEntityUpdate();
        }
        
        protected virtual void FixedUpdate()
        {
            if (!initialized)
            {
                return;
            }
            
            if (_gameManager.State != GameState.Game)
                return;

            OnEntityFixedUpdate();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_gameManager.State != GameState.Game)
                return;

            OnEntityTriggerEnter(other);
        }
        private void OnTriggerStay(Collider other)
        {
            if (_gameManager.State != GameState.Game)
                return;

            OnEntityTriggerStay(other);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (_gameManager.State != GameState.Game)
                return;

            OnEntityTriggerExit(other);
        }
    }

}