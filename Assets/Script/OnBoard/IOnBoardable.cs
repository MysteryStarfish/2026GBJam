using System;
using System.Collections.Generic;
using UnityEngine;

public interface IOnBoardable
{
    public interface IOnBoardable
    {
        GameObject gameObject { get; }
        EntityType GetEntityType();
        Vector2Int Size { get; }

        IEnumerable<Vector2Int> GetOccupiedCells(Vector2Int origin);

        void OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, OnBoardEntity other);
    }
}

public abstract class OnBoardEntity : MonoBehaviour, IOnBoardable
{
    public abstract Vector2Int Size { get; }
    public abstract EntityType GetEntityType();

    public IEnumerable<Vector2Int> GetOccupiedCells(Vector2Int origin)
    {
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                yield return new Vector2Int(origin.x + x, origin.y + y);
            }
        }
    }

    public abstract bool OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, OnBoardEntity other);
}