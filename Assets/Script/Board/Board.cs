using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("棋盤幾何配置")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 worldOrigin = Vector3.zero;

    private readonly Dictionary<Vector2Int, HashSet<OnBoardEntity>> posToEntities = new();
    private readonly Dictionary<OnBoardEntity, Vector2Int> entityOrigins = new();

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
            Mathf.RoundToInt(diff.x / cellSize),
            Mathf.RoundToInt(diff.y / cellSize)
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
}

public enum EntityType
{
    Obstacle,
    Player,
    Enemy,
    Trigger
}