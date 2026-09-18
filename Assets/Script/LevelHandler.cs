using System;
using Script.Enemy;
using Script.OnBoard.ObstacleFolder;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Script
{
    public class LevelHandler : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader _playerInput;
        [SerializeField] private EnemySpawner[] _enemySpawners;
        [SerializeField] private Obstacles _obstacles;
        [SerializeField] private Trigger _trigger;
        [SerializeField] private Board _board;
        private int currentWave = 0;

        void Start()
        {
            _trigger.Init(_board);
            var pos = _board.GetEntityPosition(_trigger);
            Debug.Log(pos);
        }

        private void OnEnable()
        {
            _playerInput.OnTest += HandleTest;
            _trigger.TriggerEvent += OnTrigger;
            for (int i = 0; i < _enemySpawners.Length-1; i++)
            {
                _enemySpawners[i].AllEnemiesDeadEvent += SpawnWave;
            }
            _enemySpawners[^1].AllEnemiesDeadEvent += OnLevelClear;
        }

        private void OnLevelClear()
        {
            _obstacles.Open();
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
            for (int i = 0; i < _enemySpawners.Length-1; i++)
            {
                _enemySpawners[i].AllEnemiesDeadEvent -= SpawnWave;
            }
            _enemySpawners[^1].AllEnemiesDeadEvent -= OnLevelClear;
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
        }
    }
}