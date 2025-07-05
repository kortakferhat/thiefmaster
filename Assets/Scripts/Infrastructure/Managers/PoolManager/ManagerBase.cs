using System;
using UnityEngine;

namespace TowerClicker.Infrastructure
{
    public class ManagerBase : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}