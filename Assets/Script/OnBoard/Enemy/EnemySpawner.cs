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

        public void StopEnemy()
        {
            // 反向迴圈，安全移除變成 null 的敵人
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy == null)
                {
                    enemies.RemoveAt(i);
                    continue;
                }
        
                enemy.Stop();
            }
        }
        
        public void MoveEnemy()
        {
            // 必須從後面往前掃描，這樣 RemoveAt 才不會導致索引錯亂
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
        
                // 嚴格過濾：只要 enemy 遺失，或是其 gameObject 已經被標記為摧毀，就移除
                if (enemy == null || enemy.gameObject == null)
                {
                    enemies.RemoveAt(i);
                    continue;
                }
        
                enemy.CanMove();
            }
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
            if (EnemyRemain <= 0)
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
                enemy.Clear();
            }
            enemies.Clear();
            EnemyRemain = 0;
        }
    }
}