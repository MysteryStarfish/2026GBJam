// using UnityEngine;
//
// public class GridDebugWindow : MonoBehaviour
// {
//     [SerializeField] private GridMover playerMover;
//     
//     private Rect windowRect = new Rect(20, 20, 200, 140);
//     private string inputX = "0";
//     private string inputY = "0";
//     private bool isVisible = true;
//
//     private void Update()
//     {
//         // 按下鍵盤反引號 ` (~ 鍵) 可以隨時開啟/關閉 Debug 視窗
//         if (Input.GetKeyDown(KeyCode.BackQuote))
//         {
//             isVisible = !isVisible;
//         }
//     }
//
//     private void OnGUI()
//     {
//         if (!isVisible || playerMover == null) return;
//
//         // 繪製一個可拖曳的視窗
//         windowRect = GUI.Window(0, windowRect, DrawDebugWindow, "Grid Teleporter");
//     }
//
//     private void DrawDebugWindow(int windowID)
//     {
//         // 顯示目前座標
//         GUILayout.Label($"Current: ({playerMover.CurrentGridPos.x}, {playerMover.CurrentGridPos.y})");
//
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("Target X:", GUILayout.Width(60));
//         inputX = GUILayout.TextField(inputX);
//         GUILayout.EndHorizontal();
//
//         GUILayout.BeginHorizontal();
//         GUILayout.Label("Target Y:", GUILayout.Width(60));
//         inputY = GUILayout.TextField(inputY);
//         GUILayout.EndHorizontal();
//
//         // 傳送按鈕
//         if (GUILayout.Button("Teleport"))
//         {
//             if (int.TryParse(inputX, out int x) && int.TryParse(inputY, out int y))
//             {
//                 Vector2Int target = new Vector2Int(x, y);
//                 bool success = playerMover.TeleportTo(target);
//                 if (!success)
//                 {
//                     Debug.LogWarning($"[Debug] Teleport to {target} failed! (Blocked or Out of Bounds)");
//                 }
//             }
//         }
//
//         // 允許滑鼠拖動視窗標題列
//         GUI.DragWindow(new Rect(0, 0, 10000, 20));
//     }
// }