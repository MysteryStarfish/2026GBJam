using DG.Tweening;
using UnityEngine;

public class BulletSlotShower : MonoBehaviour
{
    [SerializeField] private GameObject bulletA;
    [SerializeField] private GameObject bulletB;
    [SerializeField] private RectTransform[] bulletSlotPositions;

    [SerializeField] private Sprite bigBulletA;
    [SerializeField] private Sprite bigBulletB;
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
            Destroy(bullets[0]);
            bullets[0] = null;
        }

        Sequence seq = DOTween.Sequence();

        // 2. 依序將後面的元素移到前一格 (無論是不是 null 都要搬移)
        for (int i = 1; i < bullets.Length; i++)
        {
            GameObject currentBullet = bullets[i];

            if (currentBullet != null)
            {
                RectTransform targetSlot = bulletSlotPositions[i - 1];
                RectTransform bulletRect = currentBullet.GetComponent<RectTransform>();

                // 換 Parent 並播放平移動畫
                bulletRect.DOKill();
                bulletRect.SetParent(targetSlot, true);
                seq.Join(bulletRect.DOAnchorPos(Vector2.zero, 0.3f).SetEase(Ease.OutQuad));
            }

            // 重要：不管 currentBullet 是否為 null，都必須同步覆蓋到前一格！
            bullets[i - 1] = currentBullet;
        }

        // 3. 最後一格永遠清空為 null
        bullets[^1] = null;
    }

    public void RefillAll(BulletSlot bulletSlot1)
    {
        // 清除畫面上所有殘留子彈實體
        for (int i = 0; i < bullets.Length; i++)
        {
            if (bullets[i] != null) Destroy(bullets[i]);
        }

        // 重新呼叫 Init 依據當前 BulletSlot 完整生成
        Init(this.bulletSlot);
    }
}