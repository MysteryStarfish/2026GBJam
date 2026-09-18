using UnityEngine;
using System.IO;

public class MapBuilder : MonoBehaviour
{
    [Header("核心依賴")]
    public Board board;

    [Header("筆刷設定")]
    public EntityType currentBrush = EntityType.Obstacle;
    public Vector2Int brushSize = new Vector2Int(1, 1);

    [Header("當前地圖資料")]
    public MapData mapData = new MapData();

    // 給編輯器呼叫：放置物件 (目前先示範障礙物)
    public void PaintObstacle(Vector2Int cellPos)
    {
        // 檢查是否已經有東西了 (簡單示範，你未來可以寫更嚴謹的檢查)
        if (mapData.obstacles.Exists(o => o.position == cellPos)) return;

        mapData.obstacles.Add(new ObstacleData
        {
            position = cellPos,
            size = brushSize
        });
    }

    // 給編輯器呼叫：移除物件
    public void Erase(Vector2Int cellPos)
    {
        mapData.obstacles.RemoveAll(o => o.position == cellPos);
    }

    // 存檔成 JSON
    public void SaveMap(string fileName)
    {
        string json = JsonUtility.ToJson(mapData, true);
        string path = Path.Combine(Application.dataPath, $"{fileName}.json");
        File.WriteAllText(path, json);
        Debug.Log($"地圖已存檔至: {path}");
    }

    // 畫出編輯器預覽圖 (這樣在 Edit Mode 也能看到畫了什麼)
    private void OnDrawGizmos()
    {
        if (board == null) return;

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        foreach (var obs in mapData.obstacles)
        {
            Vector3 worldPos = board.CellToWorldPosition(obs.position, obs.size);
            // 根據你的 gridSize 畫出方塊 (假設你的 cellSize 是定義在 Board 裡，這裡直接抓大小)
            Gizmos.DrawCube(worldPos, new Vector3(obs.size.x, obs.size.y, 0.1f));
        }
    }
}