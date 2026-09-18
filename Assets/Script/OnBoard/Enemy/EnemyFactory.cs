using UnityEngine;

namespace Script.Enemy
{
    public enum EnemyType
    {
        Normal
    }
    public class EnemyFactory
    {
        private readonly Board _board;

        public EnemyFactory(Board board)
        {
            _board = board;
        }
        
        public Enemy CreateEnemy(Enemy prefab, Enemy.MoveInfo moveInfo, Enemy.EnemyState state)
        {
            Vector3 worldPos = _board.CellToWorldPosition(moveInfo.initPosition, moveInfo.size);
            Enemy enemyInstance = Object.Instantiate(prefab, worldPos, Quaternion.identity);

            _board.PlaceEntity(enemyInstance, moveInfo.initPosition);

            enemyInstance.Init(_board, moveInfo, state);

            return enemyInstance;
        }
    }
}