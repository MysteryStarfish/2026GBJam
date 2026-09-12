using Script.Player;
using UnityEngine;

// 掛在場景任意 Debug 物件上
public class GridSceneDebugger : MonoBehaviour
{
    // 這裡就可以直接用 SerializeField 拖曳指定！
    [SerializeField] private Board board;
    [SerializeField] private Player player;
    [SerializeField] private GameObject enemyPrefab;

    public Vector2Int targetCellPos;
    public Vector2Int spawnEnemyCellPos;

    public Board Board => board;
    public Player Player => player;
    public GameObject EnemyPrefab => enemyPrefab;
}