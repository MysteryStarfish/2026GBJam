using DG.Tweening;
using Script.Enemy;
using Script.OnBoard;
using Script.OnBoard.ObstacleFolder;
using Script.OnBoard.Player;
using UnityEngine;

namespace Script
{
    public class LevelHandler : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader _playerInput;
        [SerializeField] private EnemySpawner[] _enemySpawners;
        [SerializeField] private Obstacles _obstacles;
        [SerializeField] private Trigger _trigger;
        [SerializeField] private Board _board;
        [SerializeField] private EnergyGenerator _energyGenerator;
        [SerializeField] private IrisTransitionController _irisTransitionController;
        [SerializeField] private Player _player;
        [SerializeField] private LineFolder.Line line;
        [SerializeField] private bool _isUsed;
        [SerializeField] private GameObject _level;
        private int currentWave = 0;

        void Start()
        {
            var pos = _board.GetEntityPosition(_trigger);
            Debug.Log(pos);
        }

        private void OnEnable()
        {
            _playerInput.OnTest += HandleTest;
            _trigger.TriggerEvent += OnTrigger;
            _player.OnDeadEvent += HandleDead;
            _irisTransitionController.AllBlackEvent += HandleAllBlack;
            for (int i = 0; i < _enemySpawners.Length-1; i++)
            {
                _enemySpawners[i].AllEnemiesDeadEvent += SpawnWave;
            }
            _enemySpawners[^1].AllEnemiesDeadEvent += OnLevelClear;
        }

        private void HandleAllBlack()
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                RestartLevel();
                _irisTransitionController.PlayIrisIn();
                var pos = _board.GetEntityPosition(_trigger);
                
                _board.RemoveEntity(_player);
                _board.PlaceEntity(_player, pos);
                
                _player.PlayerPlaceInWorld(_board.CellToWorldPosition(pos, _player.Size));
                _player.PlayerCanMove();
            });
        }

        private void OnLevelClear()
        {
            _obstacles.Open();
            GameState.Instance.LevelClear();
            line.ClearBullet();
            _player.ResetInfo();
            _level.SetActive(false);
        }

        private void SpawnWave()
        {
            _enemySpawners[currentWave].SpawnEnemy();
            currentWave++;
        }

        private void OnDisable() 
        {
            _playerInput.OnTest -= HandleTest;
            _trigger.TriggerEvent -= OnTrigger;
            _player.OnDeadEvent -= HandleDead;
            _irisTransitionController.AllBlackEvent -= HandleAllBlack;
            for (int i = 0; i < _enemySpawners.Length-1; i++)
            {
                _enemySpawners[i].AllEnemiesDeadEvent -= SpawnWave;
            }
            _enemySpawners[^1].AllEnemiesDeadEvent -= OnLevelClear;
        }

        private void HandleDead()
        {
            _irisTransitionController.PlayIrisOut();
            _player.PlayerStop();
        }

        private void RestartLevel()
        {
            currentWave--;
            _enemySpawners[currentWave].Clear();
            _energyGenerator.ClearEnergy();
            OnTrigger();
            _player.PlayerResetState();
        }

        private void HandleTest()
        {
            _enemySpawners[currentWave-1].Clear();
            currentWave = 0;
            SpawnWave();
        }

        private void OnTrigger()
        {
            _obstacles.Close();
            SpawnWave();
            _energyGenerator.GenerateEnergy(6);
            GameState.Instance.StartLevel();
        }
    }
}