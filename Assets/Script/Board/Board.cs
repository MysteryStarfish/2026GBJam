using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int row;
    [SerializeField] private int col;
    [SerializeField] private int cellSize;

    public Vector3 CellToWorldPosition(Vector2 cellPosition)
    {
        return new Vector3(cellPosition.x * cellSize - 4.5f, 4f - cellPosition.y * cellSize, 0);
    }
}