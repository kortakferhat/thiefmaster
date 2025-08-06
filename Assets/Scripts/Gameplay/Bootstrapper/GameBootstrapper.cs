using Gameplay.Graph;
using Infrastructure.Components;
using Infrastructure.Managers;
using Infrastructure.Managers.EconomyManager;
using Infrastructure.Managers.LevelManager;
using Infrastructure.Managers.TooltipManager;
using TowerClicker.Infrastructure;
using TowerClicker.Infrastructure.Managers.CameraManager;
using UnityEngine;

namespace Gameplay.Bootstrapper
{
    public class GameBootstrapper : MonoBehaviour, IBootstrapper
    {
        private Transform managersParent;
        [SerializeField] private Camera topCamera;
        [SerializeField] private Gameplay.Character.CharacterController characterController;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private Transform viewRoot;
        [SerializeField] private Transform popupRoot;
        [SerializeField] private Transform tooltipsRoot;
        
        private MainMenuBootstrapper mainMenuBootstrapper;
        
        private void Awake()
        {
            managersParent = new GameObject("Managers").transform;
            mainMenuBootstrapper = GetComponent<MainMenuBootstrapper>();
        }

        private void Start()
        {
            Initialize();
        }

        public async void Initialize()
        {
            var gameManager = new GameManager();
            gameManager.Initialize();
            ServiceLocator.Register<IGameManager>(gameManager);
            
            var economyManager = new EconomyManager(0);
            ServiceLocator.Register<IEconomyManager>(economyManager);
            
            var levelManager = new LevelManager();
            await levelManager.Initialize();
            ServiceLocator.Register<ILevelManager>(levelManager);
            
            var poolManager = Instantiate(new GameObject("PoolManager"), managersParent).AddComponent<PoolManager>();
            await poolManager.Initialize();
            ServiceLocator.Register<IPoolManager>(poolManager);
            
            var particleManager = Instantiate(new GameObject("ParticleManager"), managersParent).AddComponent<ParticleManager>();
            particleManager.Initialize(poolManager);
            ServiceLocator.Register<IParticleManager>(particleManager);

            var cameraManager = new CameraManager(Camera.main, topCamera);
            ServiceLocator.Register<ICameraManager>(cameraManager);
            
            var viewManager = new ViewManager(viewRoot, popupRoot);
            ServiceLocator.Register<IViewManager>(viewManager);
            
            var tooltipManager = Instantiate(new GameObject("TooltipManager"), managersParent).AddComponent<TooltipManager>();
            tooltipManager.Initialize(poolManager, tooltipsRoot);
            ServiceLocator.Register<ITooltipManager>(tooltipManager);
            
            var turnManager = new TurnManager();
            turnManager.Initialize();
            ServiceLocator.Register<ITurnManager>(turnManager);
            
            var gridEnemyManager = Instantiate(new GameObject("GridEnemyManager"), managersParent).AddComponent<GridEnemyManager>();
            gridEnemyManager.Initialize(levelManager, gameManager);
            ServiceLocator.Register<IGridEnemyManager>(gridEnemyManager);
            
            // Initialize character after level is loaded (level is already loaded in levelManager.Initialize())
            characterController.Initialize();
            cameraFollow.Initialize();
            
            //
            
            gameManager.StartGame();
            
            mainMenuBootstrapper.Initialize();
        }
    }
}