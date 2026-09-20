using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 worldOrigin = Vector3.zero;

    private readonly Dictionary<Vector2Int, HashSet<OnBoardEntity>> posToEntities = new();
    private readonly Dictionary<OnBoardEntity, Vector2Int> entityOrigins = new();
    
    public OnBoardEntity Player { get; private set; }

    public void RegisterPlayer(OnBoardEntity player)
    {
        Player = player;
    }
    
    #region 座標換算

    public Vector3 CellToWorldPosition(Vector2Int cellPosition, Vector2Int size)
    {
        float offsetX = size.x * 0.5f * cellSize;
        float offsetY = (size.y - 1) * 0.5f * cellSize;

        return worldOrigin + new Vector3(
            cellPosition.x * cellSize + offsetX,
            cellPosition.y * cellSize + offsetY,
            0f
        );
    }

    public Vector2Int WorldToCellPosition(Vector3 worldPos)
    {
        Vector3 diff = worldPos - worldOrigin;
        
        return new Vector2Int(
            Mathf.FloorToInt((diff.x / cellSize) + 0.001f),
            Mathf.FloorToInt((diff.y / cellSize) + 0.001f)
        );
    }

    #endregion

    #region 實體管理 (放置 / 移除 / 查詢)

    public bool CanFit(OnBoardEntity entity, Vector2Int targetOrigin)
    {
        foreach (var cell in entity.GetOccupiedCells(targetOrigin))
        {
            if (posToEntities.TryGetValue(cell, out var occupants))
            {
                if (occupants.Any(o => o.GetEntityType() != EntityType.Trigger))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void PlaceEntity(OnBoardEntity entity, Vector2Int targetOrigin)
    {
        var targetCells = entity.GetOccupiedCells(targetOrigin).ToList();

        entityOrigins[entity] = targetOrigin;
        foreach (var cell in targetCells)
        {
            if (!posToEntities.TryGetValue(cell, out var set))
            {
                set = new HashSet<OnBoardEntity>();
                posToEntities[cell] = set;
            }
            set.Add(entity);
        }

        if (entity.GetEntityType() == EntityType.Player)
        {
            RegisterPlayer(entity);
        }
    }

    public void RemoveEntity(OnBoardEntity entity)
    {
        if (!entityOrigins.TryGetValue(entity, out var origin)) return;

        foreach (var cell in entity.GetOccupiedCells(origin))
        {
            if (posToEntities.TryGetValue(cell, out var set))
            {
                set.Remove(entity);
                if (set.Count == 0) posToEntities.Remove(cell);
            }
        }
        entityOrigins.Remove(entity);
    }

    public Vector2Int GetEntityPosition(OnBoardEntity entity)
    {
        return entityOrigins.TryGetValue(entity, out var pos) ? pos : throw new Exception("Entity not registered on board!");
    }

    #endregion

    #region 移動與碰撞判定

    public bool TryMove(OnBoardEntity entity, Vector2Int targetOrigin)
    {
        if (!entityOrigins.TryGetValue(entity, out var currentOrigin)) return false;

        var currentCells = new HashSet<Vector2Int>(entity.GetOccupiedCells(currentOrigin));
        var targetCells = entity.GetOccupiedCells(targetOrigin).ToList();

        var solidBlockers = new HashSet<OnBoardEntity>();
        var triggers = new HashSet<OnBoardEntity>();

        foreach (var cell in targetCells)
        {
            // 排除自身原本佔有的格子（防止大怪往旁邊移時自撞卡死）
            if (currentCells.Contains(cell)) continue;

            if (posToEntities.TryGetValue(cell, out var occupants))
            {
                foreach (var occ in occupants)
                {
                    if (occ == entity) continue;

                    triggers.Add(occ);
                }
            }
        }

        bool canMove = true;
        foreach (var trig in triggers)
        {
            trig.OnOverlapWith(targetOrigin, targetOrigin, entity);
            canMove = entity.OnOverlapWith(currentOrigin, targetOrigin, trig);
        }
        
        if (!canMove) return false;
        // 成功移動：更新格子資訊
        RemoveEntity(entity);
        PlaceEntity(entity, targetOrigin);
        return true;
    }

    #endregion
    
    /// <summary>
    /// 取得目前棋盤上所有未被非 Trigger 實體佔據的座標
    /// </summary>
    public List<Vector2Int> GetEmptyCells(Vector3 originWorldPos, Vector2Int boardSize)
    {
        var cellPosition = WorldToCellPosition(originWorldPos);
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int x = cellPosition.x; x < cellPosition.x + boardSize.x; x++)
        {
            for (int y = cellPosition.y; y < cellPosition.y + boardSize.y; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                // 檢查該格是否有非 Trigger 的實體擋住
                if (!posToEntities.TryGetValue(pos, out var occupants) ||
                    !(occupants.Any(o => (o.GetEntityType() == EntityType.Player || o.GetEntityType() == EntityType.Energy))))
                {
                    emptyCells.Add(pos);
                }
            }
        }
        return emptyCells;
    }

    /// <summary>
    /// 隨機取得一格空位，若無空位回傳 false
    /// </summary>
    public bool TryGetRandomEmptyCell(Vector3 originWorldPos, Vector2Int boardSize, out Vector2Int randomCell)
    {
        var empties = GetEmptyCells(originWorldPos, boardSize);
        if (empties.Count == 0)
        {
            randomCell = Vector2Int.zero;
            return false;
        }

        randomCell = empties[UnityEngine.Random.Range(0, empties.Count)];
        return true;
    }
}

public enum EntityType
{
    Obstacle,
    Player,
    Enemy,
    Trigger,
    Energy
}