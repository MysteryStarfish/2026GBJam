using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class Board : MonoBehaviour
{
    [SerializeField] private int row;
    [SerializeField] private int col;
    [SerializeField] private int cellSize;
    
    private Dictionary<Vector2Int, List<IOnBoardable>> posToEntity;
    private Dictionary<IOnBoardable, Vector2Int> entityPositions;

    public Vector3 CellToWorldPosition(Vector2 cellPosition) => new Vector3(cellPosition.x * cellSize - 4.5f, 4f - cellPosition.y * cellSize, 0);
    
    void SetEntityPositions(IOnBoardable entity, Vector2Int position) {
        entityPositions[entity] = position;
        if (!posToEntity.TryGetValue(position, out var list))
        {
            list = new List<IOnBoardable>();
            posToEntity[position] = list;
        }
        posToEntity[position].Add(entity);
    }
    void ChangeEntityPositions(IOnBoardable entity, Vector2Int oldPosition, Vector2Int newPosition) {
        if (posToEntity.TryGetValue(oldPosition, out var oldList))
        {
            oldList.Remove(entity);
            if (oldList.Count == 0)
            {
                posToEntity.Remove(oldPosition);
            }
        }
        SetEntityPositions(entity, newPosition);
    }
    
    void Awake()
    {
        posToEntity = new Dictionary<Vector2Int, List<IOnBoardable>>();
        entityPositions =  new Dictionary<IOnBoardable, Vector2Int>();
    }

    public void InitEntityPosition(IOnBoardable entity, Vector2Int position)
    {
        if (posToEntity.ContainsKey(position)) throw new Exception($"Position {position} is already occupied");
        
        SetEntityPositions(entity, position);
    }
    
    public void InitEntityPosition(IOnBoardable entity, Vector2Int position, Vector2Int size)
    {
        List<Vector2Int> occupiedPositions = new List<Vector2Int>(size.x * size.y);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int pos = new Vector2Int(position.x + x, position.y + y);
                
                if (posToEntity.ContainsKey(pos))
                {
                    throw new Exception($"Position {pos} is already occupied");
                }
                occupiedPositions.Add(pos);
            }
        }

        entityPositions[entity] = position;
        foreach (var pos in occupiedPositions)
        {
            if (!posToEntity.TryGetValue(pos, out var list))
            {
                list = new List<IOnBoardable>();
                posToEntity[pos] = list;
            }
            posToEntity[pos].Add(entity);
        }
    }
    
    public Vector2Int GetEntityPosition(IOnBoardable entity) => entityPositions.TryGetValue(entity, out Vector2Int pos) ? pos : throw new Exception($"Entity at position {entity} not found");
    public void TryMove(IOnBoardable entity, Vector2Int targetPos)
    {
        if (!entityPositions.TryGetValue(entity, out Vector2Int currentPos)) return;

        if (posToEntity.TryGetValue(targetPos, out var targets) && targets.Count > 0)
        {
            var targetsCopy = new List<IOnBoardable>(targets);
            foreach (var target in targetsCopy)
            {
                target.OnOverlapWith(currentPos, targetPos, entity);
                entity.OnOverlapWith(currentPos, targetPos, target);
            }
        }
        else
        {
            ChangeEntityPositions(entity, currentPos, targetPos);
        }
    }

    public void ForceMove(IOnBoardable entity, Vector2Int targetPos)
    {
        if (!entityPositions.TryGetValue(entity, out Vector2Int currentPos)) return;

        ChangeEntityPositions(entity, currentPos, targetPos);
    }
    
    public void RemoveEntity(IOnBoardable entity)
    {
        if (entityPositions.TryGetValue(entity, out Vector2Int pos))
        {
            if (posToEntity.TryGetValue(pos, out var list))
            {
                list.Remove(entity);
                if (list.Count == 0) posToEntity.Remove(pos);
            }
            entityPositions.Remove(entity);
        }
    }
}

public enum EntityType
{
    Obstacle,
    Player,
    Enemy,
}

public interface IOnBoardable
{
    GameObject gameObject { get; }
    EntityType GetEntityType();
    
    void OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, IOnBoardable other);
}