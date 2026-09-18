using NUnit.Framework.Internal;
using Script.Line;
using Script.OnBoard.Player;
using Unity.VisualScripting;
using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField] private PlayerInputReader playerInputReader;
    [SerializeField] private PlayerDetector playerDetector;
    [SerializeField] private Player player;
    [SerializeField] private BulletSlotShower bulletSlotShower;
    
    [SerializeField] private int bulletCount = 6;
    private BulletSlot bulletSlot;
    
    [SerializeField] private Vector2 startPos;
    [SerializeField] private float moveDistance;
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool moveHorizontally;

    [SerializeField] private bool isCanvaMove;
    private RectTransform rectTransform;

    private void Start()
    {
        if (isCanvaMove) rectTransform = GetComponent<RectTransform>();
        bulletSlot = new BulletSlot();
        for (int i = 0; i < bulletCount; i++)
        {
            bulletSlot.GenerateBullet();
        }

        bulletSlotShower.Init(bulletSlot);
    }
    
    public void RefillAll()
    {
        bulletSlot.Clear();
        for (int i = 0; i < bulletCount; i++)
        {
            bulletSlot.GenerateBullet();
        }
        bulletSlotShower.RefillAll(bulletSlot);
    }

    private void OnEnable()
    {
        playerInputReader.OnAttack += HandleAttack;
        playerInputReader.OnTest += HandleTest;
    }

    private void HandleTest()
    {
        RefillAll();
    }

    private void OnDisable()
    {
        playerInputReader.OnAttack -= HandleAttack;
        playerInputReader.OnTest -= HandleTest;
    }

    private void FixedUpdate()
    {
        if (!isCanvaMove) Movement();
        else CanvaMovement();
    }

    private void Movement()
    {
        var offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);

        transform.position = moveHorizontally ? new Vector3(startPos.x + offset, startPos.y, 0) : new Vector3(startPos.x, startPos.y + offset, 0);
    }
    
    private Vector2 initialPos = new Vector2(2.5f, 0);

    private void CanvaMovement()
    {
        var _moveDistance = 155;  
        var offset = Mathf.PingPong(Time.time * moveSpeed, _moveDistance);

        rectTransform.anchoredPosition = moveHorizontally ? new Vector3(initialPos.x + offset, initialPos.y, 0) : new Vector3(initialPos.x, initialPos.y + offset, 0);
    }
    
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = Application.isPlaying ? startPos : transform.position;
        Gizmos.color = Color.cyan;
        if (moveHorizontally)
        {
            Gizmos.DrawLine(origin, origin + Vector3.right * moveDistance);
            Gizmos.DrawWireSphere(origin, 0.1f);
            Gizmos.DrawWireSphere(origin + Vector3.right * moveDistance, 0.1f);
        }
        else
        {
            Gizmos.DrawLine(origin, origin + Vector3.up * moveDistance);
            Gizmos.DrawWireSphere(origin, 0.1f);
            Gizmos.DrawWireSphere(origin + Vector3.up * moveDistance, 0.1f);
        }
    }

    private void HandleAttack(BulletType attackType)
    {
        if (GetBullet().GetAttackType() != attackType)
        {
            ShotFail();
            return;
        };
        
        ShotSuccess();
    }

    private Bullet GetBullet()
    {
        return bulletSlot.GetBullet();
        // else return bloodSlot.GetBullet();
    }

    private void ShotSuccess()
    {
        if (isCanvaMove)
        {
            var screenPos = rectTransform.position;
            if (Camera.main == null) return;
            screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
            var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0f;
            playerDetector.DetectedAndHit(worldPos);
        }
        else
        {
            playerDetector.DetectedAndHit();
        }

        UseBullet();
    }

    private void UseBullet()
    {
        bulletSlot.UseBullet();
        bulletSlotShower.UpdateBullet();
    }

    private void ShotFail()
    {
        UseBullet();
        Debug.Log("Failed!");
    }
}