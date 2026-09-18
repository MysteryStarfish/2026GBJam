using UnityEngine;

namespace Script.Enemy
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] public Enemy EnemyPrefab;
        [SerializeField] public Enemy.MoveInfo moveInfo;
        [SerializeField] public Enemy.EnemyState enemyState;
    }
}