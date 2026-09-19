using UnityEngine;

public class TimeStopEffectController : MonoBehaviour
{
    [Header("請把設定在 Renderer Feature 的同一個材質球拖進來")]
    public Material fullScreenMaterial;
    
    [Header("特效擴散速度")]
    public float expandSpeed = 2.0f; 

    private float currentProgress = 0f;
    private bool isExpanding = false;

    void Start()
    {
        // 遊戲開始時重置特效 (注意：這會直接修改 Project 中的材質球數值)
        if (fullScreenMaterial != null)
        {
            fullScreenMaterial.SetFloat("_Progress", 0f);
        }
    }

    void Update()
    {
        // 按下左鍵觸發時停擴散
        if (Input.GetMouseButtonDown(0))
        {
            currentProgress = 0f;
            isExpanding = true;
        }

        // 動態推動 _Progress 數值
        if (isExpanding && fullScreenMaterial != null)
        {
            currentProgress += Time.deltaTime * expandSpeed;
            
            if (currentProgress > 1.5f) // 設大於 1 確保能完全覆蓋矩形螢幕的角落
            {
                currentProgress = 1.5f;
                isExpanding = false; 
            }

            fullScreenMaterial.SetFloat("_Progress", currentProgress);
        }
    }

    void OnApplicationQuit()
    {
        // 確保離開遊戲時材質球數值歸零，以免影響編輯器狀態
        if (fullScreenMaterial != null)
        {
            fullScreenMaterial.SetFloat("_Progress", 0f);
        }
    }
}