using UnityEngine;

namespace TowerClicker.Infrastructure.Managers.CameraManager
{
    public class CameraManager : ICameraManager
    {
        private Camera _mainCamera;
        private Camera _topCamera;
        
        public CameraManager(Camera mainCamera, Camera topCamera)
        {
            _mainCamera = mainCamera;
            _topCamera = topCamera;
        }
        
        public Camera GetTopCamera()
        {
            if (_topCamera == null)
            {
                Debug.LogError("Top camera is not initialized.");
                return null;
            }
            
            return _topCamera;
        }
        
        public Camera GetMainCamera()
        {
            if (_mainCamera == null)
            {
                Debug.LogError("Main camera is not initialized.");
                return null;
            }
            
            return _mainCamera;
        }
    }
}