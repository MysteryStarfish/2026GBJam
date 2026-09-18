using Script.OnBoard.ObstacleFolder;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider2D))]
[ExecuteAlways]
public class Obstacle : OnBoardEntity
{
    [SerializeField] private Board board;
    [SerializeField] private Vector2Int size = Vector2Int.one;
    [SerializeField] private float cellSize = 1f;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D boxCollider;
    public bool IsTrigger => boxCollider != null && boxCollider.isTrigger;
    [SerializeField] private bool isTriggerBoard;

    public override Vector2Int Size => size;
    public override EntityType GetEntityType() => EntityType.Obstacle;

    private void OnValidate()
    {
        CacheComponents();
        UpdateVisualAndCollider();
    }

    private void CacheComponents()
    {
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    [ContextMenu("Force Sync Size")]
    public void UpdateVisualAndCollider()
    {
        CacheComponents();

        if (size.x <= 0) size.x = 1;
        if (size.y <= 0) size.y = 1;
        if (cellSize <= 0) cellSize = 1f;

        Vector2 totalSize = new Vector2(size.x * cellSize, size.y * cellSize);

        if (boxCollider != null)
        {
#if UNITY_EDITOR
            // 記錄 Undo，確保 Editor 序列化儲存此改動，避免重繪時彈回
            Undo.RecordObject(boxCollider, "Update Collider Size");
#endif
            // 關鍵：關閉 AutoTiling，防止 Unity 引擎內部自動還原為圖片原尺寸
            boxCollider.autoTiling = false;
            boxCollider.size = totalSize;
            boxCollider.offset = Vector2.zero;
        }

        if (spriteRenderer != null && spriteRenderer.drawMode != SpriteDrawMode.Simple)
        {
#if UNITY_EDITOR
            Undo.RecordObject(spriteRenderer, "Update Sprite Size");
#endif
            spriteRenderer.size = totalSize;
        }
    }

    void Start()
    {
        UpdateVisualAndCollider();

        // 避免在 Editor 非播放模式下執行註冊到 Board 的行為
        if (!Application.isPlaying) return;

        if (board == null) board = FindObjectOfType<Board>();
        if (board != null && boxCollider != null)
        {
            Vector2Int gridPos = board.WorldToCellPosition(transform.position - (Vector3)boxCollider.size * 0.5f) + Vector2Int.up;
            board.PlaceEntity(this, gridPos);
            transform.position = board.CellToWorldPosition(gridPos, Size);
        }
    }

    public override bool OnOverlapWith(Vector2Int originPos, Vector2Int targetPos, OnBoardEntity other) => true;

    public void Open()
    {
        if (!isTriggerBoard || boxCollider == null) return;
        boxCollider.isTrigger = true;
    }

    public void Close()
    {
        if (!isTriggerBoard || boxCollider == null) return;
        boxCollider.isTrigger = false;
    }
}