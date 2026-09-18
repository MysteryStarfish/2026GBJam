using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridSceneDebugger))]
public class GridSceneDebuggerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 繪製預設的欄位（包含所有 [SerializeField]）
        DrawDefaultInspector();

        GridSceneDebugger debugger = (GridSceneDebugger)target;
        if (!debugger.Board) return;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Snap Player To Cell"))
        {
            if (debugger.Player)
            {
                Undo.RecordObject(debugger.Player.transform, "Move Player");
                debugger.Player.transform.position = debugger.Board.CellToWorldPosition(debugger.targetCellPos, Vector2Int.one);
            }
        }

        if (GUILayout.Button("Spawn Enemy In Scene"))
        {
            if (debugger.EnemyPrefab)
            {
                Vector3 worldPos = debugger.Board.CellToWorldPosition(debugger.spawnEnemyCellPos, debugger.enemySize);
                GameObject spawned = (GameObject)PrefabUtility.InstantiatePrefab(debugger.EnemyPrefab);
                spawned.transform.position = worldPos;
                Undo.RegisterCreatedObjectUndo(spawned, "Spawn Enemy");
            }
        }
    }
}