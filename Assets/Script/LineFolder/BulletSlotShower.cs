using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BulletSlotShower : MonoBehaviour
{
    [SerializeField] private GameObject bulletA;
    [SerializeField] private GameObject bulletB;
    [SerializeField] private RectTransform[] bulletSlotPositions;

    [SerializeField] private Sprite bigBulletA;
    [SerializeField] private Sprite bigBulletB;
    [SerializeField] private Image bigBulletImage;
    [SerializeField] private RectTransform bigBulletPosition;
    
    [SerializeField] private GameObject[] bullets;
    private BulletSlot bulletSlot;

    public void Init(BulletSlot bulletSlot)
    {
        bullets = new GameObject[bulletSlotPositions.Length];
        this.bulletSlot = bulletSlot;
        
        for (int i = 0; i < bulletSlotPositions.Length; i++)
        {
            var bulletData = bulletSlot.GetBullet(i);
            if (bulletData == null) continue;

            GameObject prefab = bulletData.GetAttackType() == BulletType.A ? bulletA : bulletB;
            bullets[i] = GenerateBullet(bulletSlotPositions[i], prefab);
        }
        
        var nextBulletData = bulletSlot.GetBullet(0);
        
        bigBulletImage.DOKill();

        if (nextBulletData != null)
        {
            bigBulletImage.sprite = nextBulletData.GetAttackType() == BulletType.A ? bigBulletA : bigBulletB;
            bigBulletImage.color = new  Color(1f, 1f, 1f, 1f);
        }
    }

    private GameObject GenerateBullet(RectTransform bulletPos, GameObject bulletPrefab)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletPos);
        RectTransform rt = bullet.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
        }
        return bullet;
    }

    public void UpdateBullet()
    {
        // 1. 銷毀被消耗的第 0 顆
        if (bullets[0] != null)
        {
            if (bullets[0].TryGetComponent<RectTransform>(out var rect0))
            {
                rect0.DOKill();
            }
            Destroy(bullets[0]);
            bullets[0] = null;
        }

        // 2. 依序將後面的元素移到前一格 (保留滑動動畫)
        for (int i = 1; i < bullets.Length; i++)
        {
            GameObject currentBullet = bullets[i];

            if (currentBullet != null)
            {
                RectTransform targetSlot = bulletSlotPositions[i - 1];
                RectTransform bulletRect = currentBullet.GetComponent<RectTransform>();

                // 切換前確實砍掉舊動畫
                bulletRect.DOKill();
                bulletRect.SetParent(targetSlot, true);
                
                // 直接呼叫 DOAnchorPos，獨立的 Tween 才能被 SetLink 完美保護
                bulletRect.DOAnchorPos(Vector2.zero, 0.3f)
                    .SetEase(Ease.OutQuad)
                    .SetLink(currentBullet);
            }

            bullets[i - 1] = currentBullet;
        }

        // 先把最後一格預設為空，等待下方的資料同步檢查
        bullets[^1] = null;

        // 3. 【關鍵修正】：根據真實的 bulletSlot 資料同步 UI
        for (int i = 0; i < bulletSlotPositions.Length; i++)
        {
            var actualBulletData = bulletSlot.GetBullet(i);
            
            // 如果資料層有子彈，但 UI 層這格是空的 (例如移位後空出的最後一格)
            if (actualBulletData != null && bullets[i] == null)
            {
                GameObject prefab = actualBulletData.GetAttackType() == BulletType.A ? bulletA : bulletB;
                bullets[i] = GenerateBullet(bulletSlotPositions[i], prefab);
                
                // 加入一個彈出的放大動畫，讓新補充的子彈視覺更自然
                bullets[i].transform.localScale = Vector3.zero;
                bullets[i].transform.DOScale(Vector3.one, 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetLink(bullets[i]);
            }
            // 防呆：如果資料層沒子彈，但 UI 卻有殘留，將其清除
            else if (actualBulletData == null && bullets[i] != null)
            {
                if (bullets[i].TryGetComponent<RectTransform>(out var rect)) rect.DOKill();
                Destroy(bullets[i]);
                bullets[i] = null;
            }
        }
        
        // 4. 大子彈圖示的動畫切換
        var nextBulletData = bulletSlot.GetBullet(0);
        
        // 停止之前的動畫，避免快速連發時動畫衝突
        bigBulletImage.DOKill();

        if (nextBulletData != null)
        {
            bigBulletImage.sprite = nextBulletData.GetAttackType() == BulletType.A ? bigBulletA : bigBulletB;
            bigBulletImage.color = new Color(1f, 1f, 1f, 0f);
            DOVirtual.DelayedCall(0.2f, () =>
            {
               bigBulletImage.color = new Color(1f, 1f, 1f, 1f);
            });
        }
        else
        {
            bigBulletImage.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    public void RefillAll(BulletSlot newBulletSlot)
    {
        // 確保如果大圖示正在做動畫，也一併終止
        bigBulletImage.DOKill();

        // 清除畫面上所有殘留子彈實體
        for (int i = 0; i < bullets.Length; i++)
        {
            if (bullets[i] != null) 
            {
                // 【關鍵防護】銷毀前一律先 DOKill
                if (bullets[i].TryGetComponent<RectTransform>(out var rect))
                {
                    rect.DOKill();
                }
                Destroy(bullets[i]);
            }
        }

        Init(newBulletSlot); 
    }
}