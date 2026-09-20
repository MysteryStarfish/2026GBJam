using System;
using System.Collections;
using UnityEngine;

public class IrisTransitionController : MonoBehaviour
{
    public Material irisMaterial;
    public Transform player;
    public float transitionDuration = 1.0f;
    public event Action AllBlackEvent;
    
    // 1.5 通常足以涵蓋整個螢幕，視比例可微調
    private float maxRadius = 1.5f; 

    void Update()
    {
        if (player != null && Camera.main != null)
        {
            // 將玩家世界座標轉換為 0~1 的視口座標
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(player.position);
            irisMaterial.SetVector("_Center", new Vector2(viewportPos.x, viewportPos.y));
        }
    }

    // 呼叫此方法來執行 Iris Out (縮小變黑)
    public void PlayIrisOut()
    {
        StartCoroutine(AnimateRadius(maxRadius, 0f));
    }

    // 呼叫此方法來執行 Iris In (放大變亮)
    public void PlayIrisIn()
    {
        StartCoroutine(AnimateRadius(0f, maxRadius));
    }

    private IEnumerator AnimateRadius(float startRadius, float endRadius)
    {
        float time = 0;
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float currentRadius = Mathf.Lerp(startRadius, endRadius, time / transitionDuration);
            irisMaterial.SetFloat("_Radius", currentRadius);
            yield return null;
        }
        irisMaterial.SetFloat("_Radius", endRadius);
        if (endRadius <= 0) AllBlackEvent?.Invoke();
    }
}