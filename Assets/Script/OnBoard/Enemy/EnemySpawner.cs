using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private EnemySpawnInfo[] enemySpawnInfo;
        private List<Enemy> enemies;
        private int EnemyRemain = 0;
        private EnemyFactory enemyFactory;
        public event Action AllEnemiesDeadEvent;

        private void Awake()
        {
            enemyFactory = new EnemyFactory(board);
            enemies = new List<Enemy>();
        }
        
        // private void Start()
        // {
        //     SpawnEnemy();
        // }

        [System.Serializable]
        struct EnemySpawnInfo
        {
            public EnemyData EnemyData;
        }

        public void SpawnEnemy()
        {
            foreach (var enemyInfo in enemySpawnInfo)
            {
                var enemy = enemyFactory.CreateEnemy(enemyInfo.EnemyData.EnemyPrefab, enemyInfo.EnemyData.moveInfo, enemyInfo.EnemyData.enemyState);
                enemy.EnemyDeadEvent += OnEnemyDied;
                enemies.Add(enemy);
                EnemyRemain++;
            }
        }

        void OnEnemyDied()
        {
            EnemyRemain--;
            if (EnemyRemain == 0)
            {
                AllEnemiesDead();
            }
        }

        private void AllEnemiesDead()
        {
            AllEnemiesDeadEvent?.Invoke();
        }

        public void Clear()
        {
            foreach (var enemy in enemies)
            {
                enemy.Dead();
            }
        }
    }
}