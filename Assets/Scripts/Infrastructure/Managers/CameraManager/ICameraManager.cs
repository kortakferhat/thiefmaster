using UnityEngine;

namespace TowerClicker.Infrastructure.Managers.CameraManager
{
    public interface ICameraManager : IService
    {
        public Camera GetMainCamera();
        public Camera GetTopCamera();
    }
}