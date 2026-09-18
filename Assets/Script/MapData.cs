using System.Collections.Generic;
using UnityEngine;
using Script.Enemy; // 確保有引用到你的 Enemy namespace

[System.Serializable]
public class MapData
{
    public List<ObstacleData> obstacles = new List<ObstacleData>();
    public List<EnemyData> enemies = new List<EnemyData>();
}

[System.Serializable]
public class ObstacleData
{
    public Vector2Int position;
    public Vector2Int size;
}

[System.Serializable]
public class EnemyData
{
    public Enemy.MoveInfo moveInfo;
    public Enemy.EnemyState state;
}