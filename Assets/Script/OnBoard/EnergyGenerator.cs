using System.Collections.Generic;
using UnityEngine;

namespace Script.OnBoard
{
    [ExecuteAlways]
    public class EnergyGenerator : OnBoardEntity
    {
        [SerializeField] private Board board;
        [SerializeField] private Energy energy;
        [SerializeField] private Vector2Int size = Vector2Int.one;
        [SerializeField] private float cellSize = 1f;
        
        [SerializeField] private int generatorNumber = 1;
        
        Dictionary<Vector2Int, Energy> energies = new();

        public override Vector2Int Size => size;
        public override EntityType GetEntityType() => EntityType.Obstacle;

        private void OnValidate()
        {
            UpdateVisual();
        }

        [ContextMenu("Force Sync Size")]
        public void UpdateVisual()
        {
            if (size.x <= 0) size.x = 1;
            if (size.y <= 0) size.y = 1;
            if (cellSize <= 0) cellSize = 1f;
        }

        void Start()
        {
            UpdateVisual();

            if (!Application.isPlaying) return;

            if (board == null) board = FindFirstObjectByType<Board>();
        }

        public void GenerateEnergy(int number)
        {
            for (int i = 0; i < number; i++)
            {
                RandomGenerate();
            }
        }

        public void ClearEnergy()
        {
            foreach (var energy in energies.Values)
            {
                if (energy != null)
                {
                    board.RemoveEntity(energy);
                    // 建議加上 .gameObject，確保整個物件都被刪除，而不只是腳本
                    Destroy(energy.gameObject); 
                }
            }
    
            // 關鍵修正：清空字典！這樣下一局才不會報錯
            energies.Clear(); 
        }

        public override bool OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, OnBoardEntity other)
        {
            return false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 totalSize = new Vector3(size.x * cellSize, size.y * cellSize, 0.1f);

            // 1. 繪製生成範圍外框 (以物件 Transform 為中心)
            Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
            Gizmos.DrawCube(transform.position, totalSize);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, totalSize);

            // 2. 繪製邊界內的每一個格子
            if (board != null)
            {
                // 先算出真實的左下角座標
                Vector3 bottomLeft = transform.position - new Vector3(size.x * cellSize * 0.5f, size.y * cellSize * 0.5f, 0f);
                Vector2Int startCell = board.WorldToCellPosition(bottomLeft);

                Gizmos.color = new Color(1f, 0.92f, 0.016f, 0.3f);
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        Vector2Int cell = new Vector2Int(startCell.x + x, startCell.y + y);
                        Vector3 worldCellCenter = board.CellToWorldPosition(cell, Vector2Int.one);
                        
                        // 關鍵修正：因為 Board 的 CellToWorldPosition(1x1) 回傳的是底部，
                        // Gizmos 需要正中心，所以我們把 Y 軸往上推半格來畫圖
                        worldCellCenter.y += cellSize * 0.5f;
                        
                        Gizmos.DrawWireCube(worldCellCenter, Vector3.one * (cellSize * 0.9f));
                    }
                }
            }
        }
#endif

        private void RandomGenerate()
        {
            // 關鍵修正：計算出真實的左下角世界座標，再傳給 Board 進行搜尋
            Vector3 bottomLeft = transform.position - new Vector3(size.x * cellSize * 0.5f, size.y * cellSize * 0.5f, 0f);
            
            if (board.TryGetRandomEmptyCell(bottomLeft, size, out var randomPos))
            {
                Vector3 worldPos = board.CellToWorldPosition(randomPos, new Vector2Int(1, 1));
                Energy energyInstance = Instantiate(energy, worldPos, Quaternion.identity);

                energyInstance.Init(board);
                board.PlaceEntity(energyInstance, randomPos);
                energies[randomPos] = energyInstance;
                Debug.Log(randomPos);
            }
        }
    }
}